import path from 'path';
import MiniCssExtractPlugin from 'mini-css-extract-plugin';
import autoprefixer from 'autoprefixer';
import cssnano from 'cssnano';

const isDevelopment = process.env.NODE_ENV === 'development';

export default {
    entry: './styles/index.js',
    output: {
        filename: 'dummy.js',
        path: path.resolve(process.cwd(), 'Wasabi/wwwroot/assets/css'),
        clean: true
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
            }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: 'site.css'
        })
    ],
    mode: isDevelopment ? 'development' : 'production',
    devServer: {
        devMiddleware: {
            writeToDisk: true,
        },
        hot: true,
        open: false,
    }
};
