import {defineConfig} from "@rsbuild/core";
import {pluginSass} from "@rsbuild/plugin-sass";
import {pluginCssMinimizer} from "@rsbuild/plugin-css-minimizer";
import * as path from "node:path";

const DEFAULT_PATH = path.resolve(__dirname, "Wasabi/wwwroot/assets/");

export default defineConfig({
    source: {
        entry: {
            main: {
                import: path.join(DEFAULT_PATH, "typescript/index.ts"),
                html: false
            },
            adminRegistration: {
                import: path.join(DEFAULT_PATH, "typescript/adminRegistration/index.ts"),
                html: false
            },
            styles: {
                import: path.join(DEFAULT_PATH, "styles/index.js"),
                html: false
            }
        },
    },
    output: {
        distPath: {
            root: path.join(DEFAULT_PATH, "generated"),
            js: "js",
            css: "css",
            assets: "assets",
        },
        filename: {
            js: "[name].js",
            css: "[name].css",
        },
        assetPrefix: "/assets/generated",
    },
    resolve: {
        alias: {
            '@font': path.join(DEFAULT_PATH, "fonts"),
            '@assets': DEFAULT_PATH
        }
    },
    plugins: [
        pluginSass(),
        pluginCssMinimizer()
    ],
    dev: {
        writeToDisk: true,
        assetPrefix: "/assets/generated",
    },
})