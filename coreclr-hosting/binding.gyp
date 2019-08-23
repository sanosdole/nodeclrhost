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
            "<!@(node -p \"require('node-addon-api').include\")"
        ],
        #'libraries': [],
        'dependencies': [
            "<!(node -p \"require('node-addon-api').gyp\")"
        ],
        'defines': [ 'NAPI_DISABLE_CPP_EXCEPTIONS', 'NODE_ADDON_API_DISABLE_DEPRECATED', 'NAPI_EXPERIMENTAL' ],
        #'defines': [ 'NAPI_CPP_EXCEPTIONS' ],

        'conditions': [
          ['OS=="linux"', {
            'defines': [
              'LINUX',
            ],
          }],
          ['OS=="win"', {
            'defines': [
              'WINDOWS',
            ],
          }, { # OS != "win",
            'defines': [
              'NON_WINDOWS_DEFINE', 
              'OSX'             
            ],
          }]
        ]
    }]
}