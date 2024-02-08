import "leaflet";
addEventListener('load', (event) => {
    let map = document.getElementsByClassName('folium-map')[0];
    map.on('click', function(e) {
        console.log(e.target);
    });

});

// addEventListener.apply('load', (event) => {
//     let map = document.getElementsByClassName('folium-map')[0];
//     map.addEventListener('click', (e) => {
//         console.log(e.target);
//     });
//     function ('click', function(e) {
//         const latlng = e.target._latlng;
//         let lat = e.latlng.lat.toFixed(4),
//             lng = e.latlng.lng.toFixed(4);
//         navigator.clipboard.writeText(`${lat}, ${lng}`);
//     }


//     new_mark.on('click', function(e) {
//         const latlng = e.target._latlng;
//         let lat = e.latlng.lat.toFixed(4),
//             lng = e.latlng.lng.toFixed(4);
//         navigator.clipboard.writeText(`${lat}, ${lng}`);
//     });
// });