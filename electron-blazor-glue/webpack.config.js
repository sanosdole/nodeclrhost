const path = require('path');
const webpack = require('webpack');

module.exports = (env, args) => ({
    target: 'electron-renderer',
    
    resolve: { extensions: ['.ts', '.js'] },
    devtool: args.mode === 'development' ? 'source-map' : false,    
    externals: {
        "coreclr-hosting": 'coreclr-hosting'
      },
    module: {
        noParse: /node_modules\/json-schema\/lib\/validate\.js/,
        rules: [
            { test: /\.ts?$/, loader: 'ts-loader' },
            { test: /\.node?$/, loader: 'node-loader'}
    ]
    },
    entry: {        
        'blazor.electron': './Boot.Electron.ts',
    },
    //output: { path: path.join(__dirname, '/dist', args.mode == 'development' ? '/Debug' : '/Release'), filename: '[name].js' }
    output: { 
        path: path.join(__dirname, '/dist'),
        filename: '[name].js',
        publicPath: '/dist/',
        libraryTarget: 'commonjs2'
     }    
});
