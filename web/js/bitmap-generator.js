class Point3D {
    constructor(x, y, z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

class FiveMap {
    constructor(elements, origin, width = null, height = null) {
        this._elements = elements;
        this.origin = origin || new Point3D(0, 0, 0);
        this._width = width;
        this._height = height;
    }

    get width() {
        if (this._width == null) {
            this.processWidthAndHeight();
        }
        return this._width;
    }

    get height() {
        if (this._height == null) {
            this.processWidthAndHeight();
        }
        return this._height;
    }

    get elements() {
        return this._elements;
    }

    processWidthAndHeight() {
        let width = 0;
        let height = 0;
        
        this._elements.forEach((element) => {
            let elementPosX = element.x + element.width() / 2;
            if (elementPosX > width) {
                width = elementPosX;
            }
            let elementPosY = element.y + element.height() / 2;
            if (elementPosY > height) {
                height = elementPosY;
            }
        });

        this.width = width;
        this.height = height;
    }
}

class Element {
    constructor(x, y, obstacle) {
        this.x = x;
        this.y = y;
        this._obstacle = obstacle;
    }

    width() {
        throw new Error("The 'width' method must be implemented in subclasses.");
    }

    height() {
        throw new Error("The 'height' method must be implemented in subclasses.");
    }
  
    isObstacle() {
        return this._obstacle == null || this._obstacle;
    }
}

class RectangleElement extends Element {
    constructor(x, y, obstacle, width, height) {
        super(x, y, obstacle);
        this._width = width;
        this._height = height;
    }

    width() {
        return this._width;
    }

    height() {
        return this._height;
    }

    draw(context, color='black') {
        context.fillStyle = color;
        context.fillRect(this.x, this.y, this._width, this._height);
    }
}

class CircleElement extends Element {
    constructor(x, y, obstacle, radius) {
        super(x, y, obstacle);
        this.radius = radius;
    }

    width() {
        return this.radius * 2;
    }

    height() {
        return this.width();
    }

    draw(context, color='black') {
        context.beginPath();
        context.arc(this.x, this.y, this.radius, 0, 2 * Math.PI);
        context.fillStyle = color;
        context.fill();
        context.closePath();
    }
}


class FiveFilesParser {
    constructor(mapConfigFiles, mapFiles) {
        this._mapConfigFiles = mapConfigFiles;
        this._mapFiles = mapFiles;
        this._mapsInfo = null;
        this._mapConfigInfo = null;
        this._fiveMap = null;
    }

    get fiveMap() {
        return this._fiveMap;
    }

    async parseFiveFiles() {
        const elements = [];

        this._mapConfigInfo = (await this.parseMapConfigFile())[0];
        console.log("Config info:", this._mapConfigInfo);

        let symbolToPrefabMap = new Map();
        this._mapConfigInfo.symbolToPrefab.forEach(entry => {
            let elementProperties = {
                shape: entry.shape || 'rectangle',
                obstacle: entry.obstacle || true,
                width: entry.width || 10,
                height: entry.height || 10,
            };
            symbolToPrefabMap.set(entry.symbol, elementProperties);
        });

        this._mapsInfo = (await this.parseMapFiles()[0]);
        console.log("Maps info:", this._mapsInfo);
        // this.parseMapFiles(this._mapFiles);

        let x = this._mapConfigInfo.origin.x;
        let z = this._mapConfigInfo.origin.z;
        this._mapsInfo.forEach((line) => {
            for (let i = 0; i < line.length; i++) {
                let symbol = line[i];
                let elementProperties = symbolToPrefabMap.get(symbol);
                let el = null;
                if (elementProperties.shape == 'circle') {

                } else {
                    el = new RectangleElement(elementProperties)
                }
                elements.push(el);
            }
        });

        let origin = this._mapConfigInfo.origin;
        this._fiveMap = new FiveMap(elements, new Point3D(origin.x, origin.y, origin.z));
        return this._fiveMap;
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
            let parsedFileArray = []; 
            dataArray.forEach(array => {
                parsedFileArray.push(array.split(/\r\n|\r|\n/));
            });
            return parsedFileArray; // 2D Array [ ["A A A A", "A B A B"], [" C C C", "B A B A"] ]
        } catch (error) {
            console.error(error);
            return null;
        }   
    }
}

class BitmapGenerator {
    constructor(filename, width, height, elements) {
        this.filename = filename;
        this.width = width;
        this.height = height;
        this.elements = elements;
    }

    generate() {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');

        canvas.width = this.width;
        canvas.height = this.height;

        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        this.elements.forEach((element) => {
            // Draw black rectangle
            ctx.fillStyle = 'black';
            if (element.shape === 'square')
                ctx.fillRect(
                    element.x, element.y,
                    element.width, element.height
                );
            else if (element.shape === 'circle')
                this.drawCircle(ctx, element.x, element.y, element.width);
        });

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

        this.download(this.filename, canvas);
        canvas.remove();
    }

    drawCircle(context, x, y, radius, color = 'black') {
        context.fillStyle = color;
        context.beginPath();
        context.arc(x, y, radius, 0, 2 * Math.PI);
        context.fill();
      }

    download(filename, canvas) {
        // Convert the canvas to a data URL
        const dataURL = canvas.toDataURL('image/bmp');

        // Download bitmap
        const downloadLink = document.createElement('a');
        downloadLink.href = dataURL;
        downloadLink.download = filename;
        downloadLink.click();
        downloadLink.remove();
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
        

        const fiveFilesParser = new FiveFilesParser(
            btnMapConfig.files,
            btnMaps.files
        );
        const fiveMap = await fiveFilesParser.parseFiveFiles();
        const bmGenerator = new BitmapGenerator(
            'prueba.bmp',
            fiveMap.width(),
            fiveMap.height(),
            fiveMap.elements()
        );
        bmGenerator.generate();
    });
});