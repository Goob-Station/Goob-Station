const fs = require("fs");
const yaml = require("js-yaml");
const axios = require("axios");

const REPO = process.env.GITHUB_REPOSITORY;
const CHANGELOG_PATH = `../../${process.env.CHANGELOG_DIR}`;
const START_DATE = new Date("2026-03-05T03:34:31.000Z");
const END_DATE = new Date(); // Now

if (process.env.GITHUB_TOKEN) {
    axios.defaults.headers.common["Authorization"] = `Bearer ${process.env.GITHUB_TOKEN}`;
}

const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\- ,]+)?\s+/im;
const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?$/img;
const CommentRegex = //gs;

async function main() {
    console.log(`Fetching PRs merged between ${START_DATE.toISOString()} and ${END_DATE.toISOString()}...`);

    let allPrs = [];
    let page = 1;
    let keepFetching = true;

    while (keepFetching) {
        const response = await axios.get(`https://api.github.com/repos/${REPO}/pulls`, {
            params: {
                state: 'closed',
                base: 'master',
                sort: 'updated',
                direction: 'desc',
                per_page: 100,
                page: page
            }
        });

        const prs = response.data;
        if (prs.length === 0) break;

        for (const pr of prs) {
            if (!pr.merged_at) continue;

            const mergedDate = new Date(pr.merged_at);

            if (mergedDate < START_DATE) {
                keepFetching = false;
                break;
            }

            if (mergedDate <= END_DATE) {
                allPrs.push(pr);
            }
        }
        page++;
    }

    allPrs.sort((a, b) => new Date(a.merged_at) - new Date(b.merged_at));

    console.log(`Found ${allPrs.length} PRs to process.`);

    let currentId = getHighestCLNumber();
    const newEntries = [];

    for (const pr of allPrs) {
        const commentlessBody = pr.body ? pr.body.replace(CommentRegex, '') : "";
        const headerMatch = HeaderRegex.exec(commentlessBody);
        
        if (!headerMatch) continue;

        let author = headerMatch[1] || pr.user.login;
        const changes = getChanges(commentlessBody);

        if (changes.length === 0) continue;

        currentId++;
        
        const formattedTime = pr.merged_at.replace("Z", ".0000000+00:00").replace("z", ".0000000+00:00");

        newEntries.push({
            author: author,
            changes: changes,
            id: currentId,
            time: formattedTime,
        });
    }

    if (newEntries.length > 0) {
        writeBatchChangelog(newEntries);
        console.log(`Successfully added ${newEntries.length} entries to the changelog.`);
    } else {
        console.log("No valid changelog entries found in the PRs within that range.");
    }
}

function getChanges(body) {
    const entries = [];
    const matches = body.matchAll(EntryRegex);

    for (const match of matches) {
        let type;
        switch (match[1].toLowerCase()) {
            case "add": type = "Add"; break;
            case "remove": type = "Remove"; break;
            case "tweak": type = "Tweak"; break;
            case "fix": type = "Fix"; break;
        }

        if (type) {
            entries.push({ type: type, message: match[2].trim() });
        }
    }
    return entries;
}

function getHighestCLNumber() {
    if (!fs.existsSync(CHANGELOG_PATH)) return 0;
    const file = fs.readFileSync(CHANGELOG_PATH, "utf8");
    const data = yaml.load(file);
    const entries = data && data.Entries ? Array.from(data.Entries) : [];
    if (entries.length === 0) return 0;
    return Math.max(...entries.map((e) => e.id), 0);
}

function writeBatchChangelog(newEntries) {
    let data = { Entries: [] };

    if (fs.existsSync(CHANGELOG_PATH)) {
        const file = fs.readFileSync(CHANGELOG_PATH, "utf8");
        const loaded = yaml.load(file);
        if (loaded && loaded.Entries) data.Entries = loaded.Entries;
    }

    data.Entries.push(...newEntries);

    const output = 
        "Name: Gooblog\n" +
        "Order: -1\n" +
        "Entries:\n" +
        yaml.dump(data.Entries, { indent: 2 }).replace(/^---/, "");

    fs.writeFileSync(CHANGELOG_PATH, output);
}

main().catch(err => {
    console.error(err);
    process.exit(1);
});
