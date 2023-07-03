#!/System/Library/Frameworks/Python.framework/Versions/2.7/Resources/Python.app/Contents/MacOS/python
import json
import sys

def httpToSshDependency(url):
    return url.replace('https://github.com', 'git+ssh://git@github.com')

if __name__ == "__main__":

    data = None

    jsonIndent = 2
    unityManifestFile = 'Packages/manifest.json'

    with (open(unityManifestFile, 'r')) as jsonFile:

        data = json.load(jsonFile)
        
        # Find realcast packages
        for key in data['dependencies']:
            if key.startswith('io.realcast.toolkit.'):
                # if url begins with https://github.com convert the string
                if data['dependencies'][key].startswith('https://github.com'):
                    #print('Need conversion for ' + data['dependencies'][key])
                    data['dependencies'][key] = httpToSshDependency(data['dependencies'][key])

        print(json.dumps(data, indent=jsonIndent))

    with (open(unityManifestFile, 'w')) as jsonFile:
        json.dump(data, jsonFile, indent=jsonIndent)
