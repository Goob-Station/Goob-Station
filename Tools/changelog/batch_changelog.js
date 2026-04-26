const fs = require("fs");
const yaml = require("js-yaml");
const axios = require("axios");

// --- Configuration ---
const REPO = process.env.GITHUB_REPOSITORY;
const CHANGELOG_DIR = process.env.CHANGELOG_DIR;
const GITHUB_TOKEN = process.env.GITHUB_TOKEN;

// Range: Now back to March 5th, 2026
const START_DATE = new Date("2026-03-05T03:34:31.000Z");
const END_DATE = new Date(); 

if (!REPO || !CHANGELOG_DIR) {
    console.error("Error: GITHUB_REPOSITORY or CHANGELOG_DIR environment variables are missing.");
    process.exit(1);
}

if (GITHUB_TOKEN) {
    axios.defaults.headers.common["Authorization"] = `Bearer ${GITHUB_TOKEN}`;
}

const CHANGELOG_PATH = `../../${CHANGELOG_DIR}`;

const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\- ,]+)?\s+/im;
const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?$/img;
const CommentRegex = //gs;

async function main() {
    console.log(`Searching for PRs merged between ${START_DATE.toISOString()} and ${END_DATE.toISOString()}...`);

    let allPrs = [];
    let page = 1;
    let keepFetching = true;

    while (keepFetching) {
        try {
            const response = await axios.get(`https://api.github.com/repos/${REPO}/pulls`, {
                params: {
                    state: 'closed',
                    sort: 'updated',
                    direction: 'desc',
                    per_page: 100,
                    page: page
                }
            });

            const prs = response.data;
            if (!prs || prs.length === 0) break;

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

            if (page > 100) break; 
        } catch (err) {
            console.error("Error fetching PRs:", err.message);
            break;
        }
    }

    if (allPrs.length === 0) {
        console.log("No merged PRs found in this date range.");
        return;
    }

    allPrs.sort((a, b) => new Date(a.merged_at) - new Date(b.merged_at));
    console.log(`Processing ${allPrs.length} PRs in chronological order...`);

    let currentId = getHighestCLNumber();
    const newEntries = [];

    for (const pr of allPrs) {
        const body = pr.body || "";
        const commentlessBody = body.replace(CommentRegex, '');
        const headerMatch = HeaderRegex.exec(commentlessBody);
        
        if (!headerMatch) continue;

        const author = headerMatch[1] || pr.user.login;
        const changes = getChanges(commentlessBody);

        if (changes.length === 0) continue;

        currentId++;
        
        const formattedTime = pr.merged_at.replace(/[Zz]/, ".0000000+00:00");

        newEntries.push({
            author: author.trim(),
            changes: changes,
            id: currentId,
            time: formattedTime,
        });
    }

    if (newEntries.length > 0) {
        writeBatchChangelog(newEntries);
        console.log(`Successfully added ${newEntries.length} new entries to the changelog.`);
    } else {
        console.log("No valid :cl: entries found in the merged PRs.");
    }
}

function getChanges(body) {
    const entries = [];
    const matches = body.matchAll(EntryRegex);

    for (const match of matches) {
        let type;
        const typeInput = match[1].toLowerCase();
        
        if (typeInput === "add") type = "Add";
        else if (typeInput === "remove") type = "Remove";
        else if (typeInput === "tweak") type = "Tweak";
        else if (typeInput === "fix") type = "Fix";

        if (type) {
            entries.push({ type: type, message: match[2].trim() });
        }
    }
    return entries;
}

function getHighestCLNumber() {
    if (!fs.existsSync(CHANGELOG_PATH)) return 0;
    try {
        const file = fs.readFileSync(CHANGELOG_PATH, "utf8");
        const data = yaml.load(file);
        const entries = (data && data.Entries) ? Array.from(data.Entries) : [];
        if (entries.length === 0) return 0;
        return Math.max(...entries.map((e) => e.id || 0), 0);
    } catch (e) {
        return 0;
    }
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
    console.error("Fatal Error:", err);
    process.exit(1);
});
