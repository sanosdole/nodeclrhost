const path = require('path');
const webpack = require('webpack');

module.exports = (env, args) => ({
    target: 'electron-renderer',
    node: {
        __dirname: false, // For whatever reason this fixes it. See: https://github.com/webpack/webpack/issues/1599
        __filename: false
      },
    resolve: { extensions: ['.ts', '.js'] },
    devtool: args.mode === 'development' ? 'source-map' : 'none',    
    module: {
        rules: [
            { test: /\.ts?$/, loader: 'ts-loader' },
            { test: /\.node?$/, loader: 'node-loader'}
    ]
    },
    entry: {        
        'blazor.electron': './Boot.Electron.ts',
    },
    //output: { path: path.join(__dirname, '/..', '/dist', args.mode == 'development' ? '/Debug' : '/Release'), filename: '[name].js' }
    output: { path: path.join(__dirname, '/dist'), filename: '[name].js' }    
});
