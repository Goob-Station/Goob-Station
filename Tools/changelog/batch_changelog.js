const fs = require("fs");
const yaml = require("js-yaml");
const axios = require("axios");

if (process.env.GITHUB_TOKEN) axios.defaults.headers.common["Authorization"] = `Bearer ${process.env.GITHUB_TOKEN}`;

const START_DATE = process.env.START_DATE || "2026-03-05T03:34:31Z";

const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\- ,]+)?\s+/im;
const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?$/img;
const CommentRegex = /<!--.*?-->/gs;

async function main() {
    if (!process.env.GITHUB_REPOSITORY) {
        console.error("GITHUB_REPOSITORY env var is required (e.g. owner/repo)");
        process.exit(1);
    }
    if (!process.env.CHANGELOG_DIR) {
        console.error("CHANGELOG_DIR env var is required");
        process.exit(1);
    }
    if (!process.env.GITHUB_TOKEN) {
        console.warn("No GITHUB_TOKEN set — unauthenticated requests are limited to 60/hour and will likely fail");
    }

    const repo = process.env.GITHUB_REPOSITORY;
    console.log(`Looking for merged PRs in ${repo} from ${START_DATE} through now`);

    const prNumbers = await getMergedPRNumbers(repo, START_DATE);
    console.log(`Search returned ${prNumbers.length} merged PRs`);
    if (prNumbers.length === 0) {
        console.log("Nothing to do");
        return;
    }

    const prs = [];
    for (const number of prNumbers) {
        try {
            const response = await axios.get(`https://api.github.com/repos/${repo}/pulls/${number}`);
            prs.push(response.data);
        } catch (err) {
            console.log(`Failed to fetch PR #${number}: ${err.message}`);
        }
    }

    prs.sort((a, b) => new Date(a.merged_at) - new Date(b.merged_at));

    let nextId = getHighestCLNumber() + 1;
    const newEntries = [];
    for (const pr of prs) {
        const entry = buildEntry(pr, nextId);
        if (entry) {
            newEntries.push(entry);
            nextId++;
        }
    }
    console.log(`Built ${newEntries.length} changelog entries from ${prs.length} PRs`);

    writeChangelog(newEntries);
    console.log(`Done. Appended ${newEntries.length} entries to ${process.env.CHANGELOG_DIR}`);
}

async function getMergedPRNumbers(repo, startDate) {
    const numbers = [];
    const perPage = 100;
    let page = 1;

    while (true) {
        const q = `repo:${repo} is:pr is:merged merged:>=${startDate}`;
        const url = `https://api.github.com/search/issues?q=${encodeURIComponent(q)}&sort=created&order=asc&per_page=${perPage}&page=${page}`;
        const response = await axios.get(url);
        const items = (response.data && response.data.items) || [];
        for (const item of items) {
            numbers.push(item.number);
        }
        if (items.length < perPage) break;
        page++;
        if (page > 10) {
            console.warn("Hit the search API's 1000-result cap; some PRs may be missing. Narrow START_DATE.");
            break;
        }
    }

    return numbers;
}

function buildEntry(pr, id) {
    const { merged_at, body, user, number } = pr;

    if (!body) {
        console.log(`PR #${number}: empty body, skipping`);
        return null;
    }

    const commentlessBody = body.replace(CommentRegex, '');

    const headerMatch = HeaderRegex.exec(commentlessBody);
    if (!headerMatch) {
        console.log(`PR #${number}: no :cl: header, skipping`);
        return null;
    }

    let author = headerMatch[1];
    if (!author) author = user.login;

    const changes = getChanges(commentlessBody);
    if (!changes || changes.length === 0) {
        console.log(`PR #${number}: header present but no entries, skipping`);
        return null;
    }

    if (!merged_at) {
        console.log(`PR #${number}: not merged, skipping`);
        return null;
    }

    const time = merged_at.replace("z", ".0000000+00:00").replace("Z", ".0000000+00:00");

    return {
        author: author,
        changes: changes,
        id: id,
        time: time,
    };
}

function getChanges(body) {
    const entries = [];
    for (const match of body.matchAll(EntryRegex)) {
        let type;
        switch (match[1].toLowerCase()) {
            case "add":    type = "Add"; break;
            case "remove": type = "Remove"; break;
            case "tweak":  type = "Tweak"; break;
            case "fix":    type = "Fix"; break;
            default: break;
        }
        if (type) {
            entries.push({ type: type, message: match[2] });
        }
    }
    return entries;
}

function getHighestCLNumber() {
    const path = `../../${process.env.CHANGELOG_DIR}`;
    if (!fs.existsSync(path)) return 0;

    const file = fs.readFileSync(path, "utf8");
    const data = yaml.load(file);
    const entries = data && data.Entries ? Array.from(data.Entries) : [];
    const clNumbers = entries.map((entry) => entry.id);
    return Math.max(...clNumbers, 0);
}

function writeChangelog(newEntries) {
    const path = `../../${process.env.CHANGELOG_DIR}`;
    let data = { Entries: [] };

    if (fs.existsSync(path)) {
        const file = fs.readFileSync(path, "utf8");
        data = yaml.load(file) || { Entries: [] };
        if (!data.Entries) data.Entries = [];
    }

    for (const entry of newEntries) {
        data.Entries.push(entry);
    }

    fs.writeFileSync(
        path,
        "Name: Gooblog\nOrder: -1\nEntries:\n" +
            yaml.dump(data.Entries, { indent: 2 }).replace(/^---/, "")
    );
}

main().catch((err) => {
    console.error("Fatal error:", err.message || err);
    process.exit(1);
});
