import {defineConfig} from "@rsbuild/core";
import {pluginSass} from "@rsbuild/plugin-sass";
import {pluginCssMinimizer} from "@rsbuild/plugin-css-minimizer";
import * as fs from "fs";
import * as path from "node:path";

const tsFolder = './Wasabi/wwwroot/assets/typescript';

function getScriptEntries() {
    const entries = {
        // Always include the main index file
        main: `${tsFolder}/index.ts`
    };

    // Read subdirectories in the typescript folder
    const dirs = fs.readdirSync(tsFolder, {withFileTypes: true})
        .filter(dirent => dirent.isDirectory())
        .map(dirent => dirent.name);

    // Add each directory as an entry point if it has an index.ts file
    dirs.forEach(dir => {
        const indexPath = `${tsFolder}/${dir}/index.ts`;
        if (fs.existsSync(indexPath)) {
            entries[dir] = indexPath;
        } else {
            // Try to find any .ts file in the directory
            const tsFiles = fs.readdirSync(`${tsFolder}/${dir}`)
                .filter(file => file.endsWith('.ts'));

            if (tsFiles.length > 0) {
                entries[dir] = `${tsFolder}/${dir}/${tsFiles[0]}`;
            }
        }
    });

    return entries;
}

export default defineConfig({
    plugins: [
        pluginSass(),
        pluginCssMinimizer()],
    source: {
        entry: {
            css: "./Wasabi/wwwroot/assets/styles/index.js",
            ...getScriptEntries()
        },
        assetsInclude: "./Wasabi/wwwroot/assets/styles/assets/fonts/*.ttf",
    },
    dev: {
        writeToDisk: true,
    },
    output: {
        distPath: {
            root: "./Wasabi/wwwroot/assets/generated",
            js: "js",
            css: "css"
        },
        filename: {
            js: "[name].js",
            css: "[name].css",
        }
    }
})