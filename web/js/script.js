// Example: download("test.json", JSON.stringify(obj, null, 4))
function download(filename, text) {
    let element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    element.setAttribute('download', filename);
  
    element.style.display = 'none';
    document.body.appendChild(element);
  
    element.click();
  
    document.body.removeChild(element);
}

function createCheckboxInput(labelText, labelRef) {
    let div = document.createElement("div");
    
    let booldiv = document.createElement("div");
    booldiv.setAttribute("class", "bool");

    let label = document.createElement("label");
    label.setAttribute("for", labelRef);
    label.appendChild(document.createTextNode(labelText));

    let input = document.createElement("input");
    input.setAttribute("name", labelRef);
    input.setAttribute("type", "checkbox");

    booldiv.appendChild(label);
    booldiv.appendChild(input);
    div.appendChild(booldiv);

    return div;
}

function createTextboxInput(labelText, labelRef, placeholder) {
    let div = document.createElement("div");
    
    let label = document.createElement("label");
    label.setAttribute("for", labelRef);
    label.appendChild(document.createTextNode(labelText));
    
    let input = document.createElement("input");
    input.setAttribute("name", labelRef);
    input.setAttribute("type", "text");
    if (placeholder != null)
        input.setAttribute("placeholder", placeholder);

    div.appendChild(label);
    div.appendChild(input);

    return div;
}

function createNumberInput(labelText, labelRef, placeholder) {
    let div = document.createElement("div");
    let [label, input] = createNumberUnit(labelText, labelRef, placeholder);
    div.appendChild(label);
    div.appendChild(input);
    return div;
}

function createNumberUnit(labelText, labelRef, placeholder) {
    let label = document.createElement("label");
    label.setAttribute("for", labelRef);
    label.appendChild(document.createTextNode(labelText));
    
    let input = document.createElement("input");
    input.setAttribute("name", labelRef);
    input.setAttribute("type", "number");
    if (placeholder != null)
        input.setAttribute("placeholder", placeholder);

    return [label, input];
}

function createMapjsonPosition() {
    let position = document.createElement("div");
    let positionTitle = document.createElement("h3");
    positionTitle.appendChild(document.createTextNode("Position"));
    position.appendChild(positionTitle);

    let div = document.createElement("div");
    div.setAttribute("class", "grid");
    let coordinates = [["x:", "x"], ["y:", "y"], ["z:", "z"]];
    coordinates.forEach(el => {
        let [label, input] = createNumberUnit(el[0], el[1]);
        div.appendChild(label);
        div.appendChild(input);
    });
    position.appendChild(div);
    return position;
}

function createMapjsonRotation() {
    let position = document.createElement("div");
    let positionTitle = document.createElement("h3");
    positionTitle.appendChild(document.createTextNode("Rotation"));
    position.appendChild(positionTitle);

    let div = document.createElement("div");
    div.setAttribute("class", "grid");
    let coordinates = [["x:", "x"], ["y:", "y"], ["z:", "z"]];
    coordinates.forEach(el => {
        let [label, input] = createNumberUnit(el[0], el[1]);
        div.appendChild(label);
        div.appendChild(input);
    });
    position.appendChild(div);
    return position;
}

function createMapjsonColor() {
    let color = document.createElement("div");
    let colorTitle = document.createElement("h3");
    colorTitle.appendChild(document.createTextNode("Color"));
    color.appendChild(colorTitle);

    let div = document.createElement("div");
    div.setAttribute("class", "grid");
    let channels = [["r:", "r"], ["g:", "g"], ["b:", "b"], ["a:", "a"]];
    channels.forEach(el => {
        let [label, input] = createNumberUnit(el[0], el[1]);
        div.appendChild(label);
        div.appendChild(input);
    });
    color.appendChild(div);
    return color;
}

// Add mapjson light element
function addNewLight() {
    let light = document.createElement("div");
    light.setAttribute("class", "light");

    let active = createCheckboxInput("Active", "active");
    let objectname = createTextboxInput("Object name", "objectName");
    let prefabname = createTextboxInput("Prefab name", "prefabName");
    let position = createMapjsonPosition();
    let rotation = createMapjsonRotation();
    let color = createMapjsonColor();  
    let intensity = createNumberInput("Intensity", "intensity");

    light.appendChild(active);
    light.appendChild(objectname);
    light.appendChild(prefabname);
    light.appendChild(position);
    light.appendChild(rotation);
    light.appendChild(color);
    light.appendChild(intensity);

    return light
}

window.addEventListener('load', function(e) {
    let btnAdd = document.getElementById("mapjsonAdd");
    btnAdd.addEventListener('click', (e) => {
        let lightContainer = document.getElementById("lights");
        lightContainer.appendChild(addNewLight());
    });
    btnAdd.click();

    let btnDownload = document.getElementById("mapjsonDownload");
    btnDownload.addEventListener('click', (e) => {
        download("map.json", JSON.stringify(demo, null, 4));
    });
});


const demo = {
    "lights": [
        {
            "active": true,
            "objectName": "Sun Light",
            "objectPrefabName": "Light",
            "position": {
                "x": 0.0,
                "y": 3.0,
                "z": 0.0
            },
            "rotation": {
                "x": 35.0,
                "y": 40.0,
                "z": 0.0
            },
            "color": {
                "r": 1.0,
                "g": 0.95,
                "b": 0.83,
                "a": 1.0
            },
            "intensity": 2.0
        },
        {
            "active": false,
            "objectName": "Moon Light",
            "objectPrefabName": "Light",
            "position": {
                "x": 0.0,
                "y": 3.0,
                "z": 0.0
            },
            "rotation": {
                "x": 154.0,
                "y": 224.0,
                "z": 0.0
            },
            "color": {
                "r": 0.51,
                "g": 0.70,
                "b": 1.0,
                "a": 1.0
            },
            "intensity": 1.4
        }
    ],
    "objects": [
        {
            "active": true,
            "objectName": "Spawner 0",
            "objectPrefabName": "Spawner",
            "position": {
                "x": 3.5,
                "y": 0.0,
                "z": 0.0
            },
            "rotation": {
                "x": 0.0,
                "y": 0.0,
                "z": 0.0
            }
        },
        {
            "active": false,
            "objectName": "Tree 1",
            "objectPrefabName": "Tree",
            "position": {
                "x": -2.5999999046325685,
                "y": 0.0,
                "z": 0.0
            },
            "rotation": {
                "x": 0.0,
                "y": 0.0,
                "z": 0.0
            }
        }
    ]
}