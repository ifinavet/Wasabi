import path from 'path';
import fs from 'fs';
import MiniCssExtractPlugin from 'mini-css-extract-plugin';
import autoprefixer from 'autoprefixer';
import cssnano from 'cssnano';
import webpack from 'webpack';
import dotenv from 'dotenv';

const isDevelopment = process.env.NODE_ENV === 'development';
const tsFolder = './Wasabi/wwwroot/assets/typescript';

dotenv.config()

// Function to get subdirectories and create entry points
function getScriptEntries() {
    const entries = {
        // Always include the main index file
        main: `${tsFolder}/index.ts`
    };

    // Read subdirectories in the typescript folder
    const dirs = fs.readdirSync(tsFolder, { withFileTypes: true })
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

export default {
    entry: {
        styles: './Wasabi/wwwroot/assets/styles/index.js',
        ...getScriptEntries()
    },
    output: {
        filename: (pathData) => {
            if (pathData.chunk.name === 'styles') {
                return 'css/dummy.js';
            }
            return `js/${pathData.chunk.name}.js`;
        },
        path: path.resolve(process.cwd(), 'Wasabi/wwwroot/assets/generated'),
        clean: true
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js', '.jsx']
    },
    module: {
        rules: [
            {
                test: /\.(sa|sc|c)ss$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                        options: {
                            url: false,
                        }
                    },
                    {
                        loader: 'postcss-loader',
                        options: {
                            postcssOptions: {
                                plugins: [
                                    autoprefixer,
                                    cssnano({ preset: 'default' })
                                ]
                            }
                        }
                    },
                    'sass-loader'
                ]
            },
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/
            }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: 'css/site.css'
        }),
        new webpack.DefinePlugin({
            POSTHOG_PROJECT_API_KEY: JSON.stringify(process.env.POSTHOG_PROJECT_API_KEY),
        })
    ],
    mode: isDevelopment ? 'development' : 'production',
    devServer: {
        devMiddleware: {
            writeToDisk: true,
        },
        hot: true,
        open: false,
    },
    optimization: {
        runtimeChunk: false,
        splitChunks: false
    }
};