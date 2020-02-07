{
    "targets": [{
        "target_name": "coreclr-hosting",
        #"cflags!": [ "-fno-exceptions" ],
        #"cflags_cc!": [ "-fno-exceptions" ],
        "sources": [
            "cppsrc/main.cc",
            "cppsrc/dotnethost.cc",
            "cppsrc/context.cc",
            "cppsrc/dotnetexports.cc"
        ],
        'include_dirs': [
            "<!@(node -p \"require('node-addon-api').include\")",
            "cppsrc",
            "cppsrc/inc",
            "./hostfxr/bin"
        ],
        #'libraries': [],
        'dependencies': [
            "<!(node -p \"require('node-addon-api').gyp\")"
        ],
        'defines': [ 'NAPI_DISABLE_CPP_EXCEPTIONS', 'NODE_ADDON_API_DISABLE_DEPRECATED', 'NAPI_EXPERIMENTAL' ],
        #'defines': [ 'NAPI_CPP_EXCEPTIONS' ],

        'conditions': [
          ['OS=="linux"', {
            #'cflags_cc!': ['-std=gnu++1y'],    
            #'cflags_cc+': ['-std=c++17'],
            'defines': [
              'NON_WINDOWS_DEFINE', 
              'LINUX'
            ],
            'link_settings': {
              'libraries': [
                "-Lhostfxr/bin"
                "-lnethost"
              ]
            },
            'copies': [
              {
                'destination': '<(PRODUCT_DIR)',
                'files': ['./hostfxr/bin/libnethost.so'],
              }
            ]
          }],
          ['OS=="win"', {
            #'msvs_settings': {
            #  'VCCLCompilerTool': {
            #    'AdditionalOptions': [ '-std:c++17', ],
            #  },
            #},
            'defines': [
              'WINDOWS',
            ],
            'link_settings': {
              'libraries': [
                "-lnethost.lib"
              ],
              'library_dirs': [
                './hostfxr/bin',
              ],
            'copies': [
              {
                'destination': '<(PRODUCT_DIR)',
                'files': ['./hostfxr/bin/nethost.dll'],
              }
            ]
            }
          }],
          ['OS=="mac"', {
            #'cflags_cc!': ['-std=gnu++1y'],    
            #'cflags_cc+': ['-std=c++17'],
            #"xcode_settings": {
            #  "OTHER_CFLAGS": [ "-std=c++17"],
            #},
            'defines': [
              'NON_WINDOWS_DEFINE', 
              'OSX'             
            ],
            'link_settings': {
              'libraries': [
                "-lnethost",
                "-Wl,-rpath,@loaderpath"
              ],
              'library_dirs': [
                './hostfxr/bin',
              ],
            'copies': [
              {
                'destination': '<(PRODUCT_DIR)',
                'files': ['./hostfxr/bin/libnethost.dylib'],
              }
            ]
            }
          }]
        ]
    }]
}