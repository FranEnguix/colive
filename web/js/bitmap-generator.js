
class ObstacleParser {
    constructor(mapConfigFiles, mapFiles) {
        this._mapConfigFiles = mapConfigFiles;
        this._mapFiles = mapFiles;
        this._obstacles = [];
        this._mapsInfo = null;
        this._mapConfigInfo = null;
    }

    async parseObstacles() {
        this._mapConfigInfo = await this.parseMapConfigFile();
        console.log("Config info:", this._mapConfigInfo);

        this._mapsInfo = await this.parseMapFiles();
        console.log("Maps info:", this._mapsInfo);
        // this.parseMapFiles(this._mapFiles);
    }

    async parseMapConfigFile() {
        const files = this._mapConfigFiles;
        if (!files || files.length === 0) {
            console.error('No files selected.');
            return;
        }
        
        const readJsonFiles = (files) => {
            return Promise.all(Array.from(files).map(file => {
                return new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = function(event) {
                        const contents = event.target.result;
                        try {
                            const jsonData = JSON.parse(contents);
                            resolve(jsonData);
                        } catch (error) {
                            reject(`Error parsing JSON in ${file.name}: ${error}`);
                        }
                    };
                    reader.readAsText(file);
                });
            }));
        };

        // (discarted) SYNCHRONOUS VERSION:
        // readJsonFiles(files)
        //     .then(data => {
        //         console.log('JSON Data:', data);
        //         return data;
        //     })
        //     .catch(error => {
        //         console.error(error);
        //         return Promise.reject(error);
        //     });

        try {
            const jsonDataArray = await readJsonFiles(files);
            return jsonDataArray;
        } catch (error) {
            console.error(error);
            return null;
        }
    }



    async parseMapFiles() {
        const files = this._mapFiles;
        if (!files || files.length === 0) {
            console.error('No files selected.');
            return;
        }
        
        const readFiles = (files) => {
            return Promise.all(Array.from(files).map(file => {
                return new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = function(event) {
                        const contents = event.target.result;
                        try {
                            // 2D Array
                            resolve(contents);
                        } catch (error) {
                            reject(`Error parsing Map file in ${file.name}: ${error}`);
                        }
                    };
                    reader.readAsText(file);
                });
            }));
        };

        try {
            const dataArray = await readFiles(files);
            return dataArray;
        } catch (error) {
            console.error(error);
            return null;
        }   
    }



    get obstacles() {
        return this._obstacles;
    }

}

class BitmapGenerator {
    constructor(filename, width, height, obstacleParser) {
        this.filename = filename;
        this.width = width;
        this.height = height;
        this.obstacleParser = obstacleParser;
    }

    

    generate() {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');

        // Set the canvas size based on the class properties
        canvas.width = this.width;
        canvas.height = this.height;

        // Fill the canvas with white
        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Draw black lines
        ctx.strokeStyle = 'black';
        ctx.lineWidth = 5;

        // Vertical line
        ctx.beginPath();
        ctx.moveTo(this.width / 2, 0);
        ctx.lineTo(this.width / 2, this.height);
        ctx.stroke();

        // Horizontal line
        ctx.beginPath();
        ctx.moveTo(0, this.height / 2);
        ctx.lineTo(this.width, this.height / 2);
        ctx.stroke();

        // Convert the canvas to a data URL
        const dataURL = canvas.toDataURL('image/bmp');

        // Create a download link
        const downloadLink = document.createElement('a');
        downloadLink.href = dataURL;
        downloadLink.download = 'black_and_white_image.bmp';
        downloadLink.click();
        downloadLink.remove();
        canvas.remove();
    }

}


function updateMapFiles(inputName) {
    const btn = document.querySelector("[name='" + inputName + "']");
    btn.addEventListener('change', () => {
        const file = btn.files[0];
        const reader = new FileReader();
        reader.addEventListener('load', (e) => {
            const json = e.target.result;
            const data = JSON.parse(json);
            return data;
        });
        return reader.readAsText(file);
    });
}


function parseJsonFiles(event) {
    const files = event.target.files;
    if (!files || files.length === 0) {
        console.error('No files selected.');
        return;
    }
    
    const readJsonFiles = (files) => {
        return Promise.all(Array.from(files).map(file => {
            return new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = function(event) {
                    const contents = event.target.result;
                    try {
                        const jsonData = JSON.parse(contents);
                        resolve(jsonData);
                    } catch (error) {
                        reject(`Error parsing JSON in ${file.name}: ${error}`);
                    }
                };
                reader.readAsText(file);
            });
        }));
    };

    readJsonFiles(files)
        .then(data => {
            console.log('JSON Data:', data);
        })
        .catch(error => {
            console.error(error);
        });
}

function parseMapFile(event) {
    const files = event.target.files;
    if (!files || files.length === 0) {
        console.error('No files selected.');
        return;
    }
    
    const readFiles = (files) => {
        return Promise.all(Array.from(files).map(file => {
            return new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = function(event) {
                    const contents = event.target.result;
                    try {
                        // 2D Array
                        resolve(contents);
                    } catch (error) {
                        reject(`Error parsing Map file in ${file.name}: ${error}`);
                    }
                };
                reader.readAsText(file);
            });
        }));
    };

    readFiles(files)
        .then(data => {
            console.log('Data:', data);
        })
        .catch(error => {
            console.error(error);
        });
}




window.addEventListener('load', function(e) {
    const btn = document.getElementById('generateImage');

    const btnMaps = document.querySelector("[name='importMapFiles']");
    // btnMaps.addEventListener('change', function(e) { parseMapFile(e); });
    const btnMapConfig = document.querySelector("[name='importMapConfigFile']");
    // btnMapConfig.addEventListener('change', function(e) { parseJsonFile(e); });
    
    btn.addEventListener('click', async function () {
        

        const obstacleParser = new ObstacleParser(
            btnMapConfig.files,
            btnMaps.files
        );
        await obstacleParser.parseObstacles();
        const bmGenerator = new BitmapGenerator(
            'prueba.bmp',
            300,
            300,
            obstacleParser
        );
        bmGenerator.generate();
    });
});