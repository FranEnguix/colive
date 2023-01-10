function updateFlapSelectedClass(newSelectedElement) {
    const selectedFlapClass = "flap-selected";
    const currentFlapSelected = document.querySelector("."+selectedFlapClass);
    currentFlapSelected.classList.remove(selectedFlapClass);
    newSelectedElement.classList.add(selectedFlapClass);

    const selectedContainerClass = "selected-config-container";
    const data_link = newSelectedElement.getAttribute("data-link");
    const newSelectedContainer = document.getElementById(data_link);
    const currentContainerSelected = document.querySelector("."+selectedContainerClass);
    currentContainerSelected.classList.remove(selectedContainerClass);
    newSelectedContainer.classList.add(selectedContainerClass);
}

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

function removeAllChildNodes(parent) {
    while (parent.firstChild) {
        parent.removeChild(parent.firstChild);
    }
}

function isOptionInSelector(selector, optionText) {
    for (const option of selector.children)
        if (option.getAttribute("value") == optionText)
            return true;
    return false;
}

function getOptionsInSelector(selector) {
    const options = [];
    for (const option of selector.children)
        options.push(option.getAttribute("value"));
    return options;
}

function updateSelectors(tagContains, optionList) {
    const selectors = document.querySelectorAll("select[name*='" + tagContains + "']");
    selectors.forEach(selector => {
        updateSelector(selector, optionList);
    });
}

function updateSelector(selector, optionList) {
    optionList.forEach(optionName => {
        if (!isOptionInSelector(selector, optionName)) {
            const option = document.createElement("option");
            option.setAttribute("value", optionName);
            option.appendChild(document.createTextNode(optionName));
            selector.appendChild(option);
        }
    });
}

function createCheckboxInput(labelText, labelRef, defaultValue) {
    const div = document.createElement("div");
    
    const booldiv = document.createElement("div");
    booldiv.setAttribute("class", "bool");

    const label = document.createElement("label");
    label.setAttribute("for", labelRef);
    label.appendChild(document.createTextNode(labelText));

    const input = document.createElement("input");
    input.setAttribute("name", labelRef);
    input.setAttribute("type", "checkbox");
    input.checked = defaultValue;

    booldiv.appendChild(label);
    booldiv.appendChild(input);
    div.appendChild(booldiv);

    return div;
}

function createTextboxInput(labelText, labelRef, placeholder) {
    const div = document.createElement("div");
    
    const label = document.createElement("label");
    label.setAttribute("for", labelRef);
    label.appendChild(document.createTextNode(labelText));
    
    const input = document.createElement("input");
    input.setAttribute("name", labelRef);
    input.setAttribute("type", "text");
    if (placeholder != null)
        input.setAttribute("placeholder", placeholder);

    div.appendChild(label);
    div.appendChild(input);

    return div;
}

function createNumberInput(labelText, labelRef, placeholder) {
    const div = document.createElement("div");
    const [label, input] = createNumberUnit(labelText, labelRef, placeholder);
    div.appendChild(label);
    div.appendChild(input);
    return div;
}

function createNumberUnit(labelText, labelRef, placeholder) {
    const label = document.createElement("label");
    label.setAttribute("for", labelRef);
    label.appendChild(document.createTextNode(labelText));
    
    const input = document.createElement("input");
    input.setAttribute("name", labelRef);
    input.setAttribute("type", "number");
    if (placeholder != null)
        input.setAttribute("placeholder", placeholder);

    return [label, input];
}

function createSelectInput(labelText, labelRef) {
    const div = document.createElement("div");
    
    const label = document.createElement("label");
    label.setAttribute("for", labelRef);
    label.appendChild(document.createTextNode(labelText));
    
    const select = document.createElement("select");
    select.setAttribute("name", labelRef);
    select.setAttribute("type", "select");

    div.appendChild(label);
    div.appendChild(select);

    return div;
}

function cloneSelectInput(selectQueryString) {
    const selectorIn = document.querySelector(selectQueryString);
    const divIn = selectorIn.parentElement;
    const labelIn = divIn.querySelector("label");
    const div = createSelectInput(
        labelIn.textContent,
        labelIn.getAttribute("for")
    );
    updateSelector(
        div.querySelector("select"), 
        getOptionsInSelector(selectorIn)
    );
    return div;
}

function createMapjsonPosition() {
    const position = document.createElement("div");
    const positionTitle = document.createElement("h4");
    positionTitle.appendChild(document.createTextNode("Position"));
    position.appendChild(positionTitle);

    const div = document.createElement("div");
    div.setAttribute("class", "grid");
    const coordinates = [["x:", "x"], ["y:", "y"], ["z:", "z"]];
    coordinates.forEach(el => {
        const [label, input] = createNumberUnit(el[0], el[1]);
        div.appendChild(label);
        div.appendChild(input);
    });
    position.appendChild(div);
    return position;
}

function createMapjsonRotation() {
    const position = document.createElement("div");
    const positionTitle = document.createElement("h4");
    positionTitle.appendChild(document.createTextNode("Rotation"));
    position.appendChild(positionTitle);

    const div = document.createElement("div");
    div.setAttribute("class", "grid");
    const coordinates = [["x:", "x"], ["y:", "y"], ["z:", "z"]];
    coordinates.forEach(el => {
        const [label, input] = createNumberUnit(el[0], el[1]);
        div.appendChild(label);
        div.appendChild(input);
    });
    position.appendChild(div);
    return position;
}

function createMapjsonColor() {
    const color = document.createElement("div");
    const colorTitle = document.createElement("h4");
    colorTitle.appendChild(document.createTextNode("Color"));
    color.appendChild(colorTitle);

    const div = document.createElement("div");
    div.setAttribute("class", "grid");
    const channels = [["r:", "r"], ["g:", "g"], ["b:", "b"], ["a:", "a"]];
    channels.forEach(el => {
        const [label, input] = createNumberUnit(el[0], el[1]);
        div.appendChild(label);
        div.appendChild(input);
    });
    color.appendChild(div);
    return color;
}

// Add mapjson light element
function addNewLight() {
    const light = document.createElement("div");
    light.setAttribute("class", "light");

    const active = createCheckboxInput("Active", "active", true);
    const objectname = createTextboxInput("Object name", "objectName");
    const prefabSelect = document.querySelector("[name='prefabName']");
    let prefabname;
    if (prefabSelect == null) {
        prefabname = createSelectInput("Prefab name", "prefabName");
    } else {
        prefabname = cloneSelectInput("[name='prefabName']");
    }
    const position = createMapjsonPosition();
    const rotation = createMapjsonRotation();
    const color = createMapjsonColor();  
    const intensity = createNumberInput("Intensity", "intensity");

    light.appendChild(active);
    light.appendChild(objectname);
    light.appendChild(prefabname);
    light.appendChild(position);
    light.appendChild(rotation);
    light.appendChild(color);
    light.appendChild(intensity);

    return light
}

function addNewObject() {
    const obj = document.createElement("div");
    obj.setAttribute("class", "object");

    const active = createCheckboxInput("Active", "active", true);
    const objectname = createTextboxInput("Object name", "objectName");
    const prefabSelect = document.querySelector("[name='prefabName']");
    let prefabname;
    if (prefabSelect == null) {
        prefabname = createSelectInput("Prefab name", "prefabName");
    } else {
        prefabname = cloneSelectInput("[name='prefabName']");
    }
    const position = createMapjsonPosition();
    const rotation = createMapjsonRotation();

    obj.appendChild(active);
    obj.appendChild(objectname);
    obj.appendChild(prefabname);
    obj.appendChild(position);
    obj.appendChild(rotation);

    return obj
}

window.addEventListener('load', function(e) {
    const btnAddLight = document.getElementById("mapjsonAddLight");
    btnAddLight.addEventListener('click', (e) => {
        const lightContainer = document.getElementById("lights");
        lightContainer.appendChild(addNewLight());
    });
    btnAddLight.click();

    const btnAddObject = document.getElementById("mapjsonAddObject");
    btnAddObject.addEventListener('click', (e) => {
        const lightContainer = document.getElementById("objects");
        lightContainer.appendChild(addNewObject());
    });
    btnAddObject.click();

    const btnDownload = document.getElementById("mapjsonDownload");
    btnDownload.addEventListener('click', (e) => {
        download("map.json", JSON.stringify(demo, null, 4));
    });

    const btnImportPrefabs = document.querySelector("[name='importPrefabs']")
    btnImportPrefabs.addEventListener('change', () => {
        const file = btnImportPrefabs.files[0];
        const reader = new FileReader();
        reader.addEventListener('load', (e) => {
            const json = e.target.result;
            const data = JSON.parse(json);
            updateSelectors("prefabName", data["prefabs"]);
        });
        reader.readAsText(file);
    });

    const flaps = document.querySelectorAll("#menu a");
    flaps.forEach(flap => {
        flap.addEventListener("click", e => {
            const elementClicked = e.target;
            updateFlapSelectedClass(elementClicked);
        });
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