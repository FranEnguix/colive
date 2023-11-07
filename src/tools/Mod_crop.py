import tkinter as tk
from PIL import Image, ImageTk

# Variables globales
_start = None    
_end = None
_img = None
_photo = None
_img_path = None
_funcids = {}
root = None
canvas = None
crop_btn = None

def _enable_croping():
    global canvas
    _funcids["<Button-1>"] = canvas.bind("<Button-1>", _on_click, '+')
    _funcids["<B1-Motion>"] = canvas.bind("<B1-Motion>", _on_drag, '+')
    _funcids["<ButtonRelease-1>"] = canvas.bind("<ButtonRelease-1>", _on_drop, '+')

def _disable_croping():
    global canvas
    global crop_btn
    for event, funcid in _funcids.items():
        canvas.unbind(event, funcid)
    crop_btn.config(state="disabled")


def _open_image(name):
    global _img
    global _photo
    global _img_path
    global canvas
    global root
    #file = tk.filedialog.askopenfilename(parent=root, initialdir="M:/",title='Choose an image.')
#     if not file:
#         return None
    file=name
    canvas.delete("all")
    try:
        _img = Image.open(file)
    except OSError:
        _img = None
        _photo = None
        _img_path = None
        _disable_croping()
    else:
        _photo = ImageTk.PhotoImage(_img)
        _img_path = file
        canvas.create_image(0, 0, image=_photo, anchor="nw", tags="image")
        _enable_croping()
    finally:
        canvas.config(scrollregion=canvas.bbox(tk.ALL))


def _on_click(event):
    global _start
    global _end
    global canvas
    _start = (canvas.canvasx(event.x), canvas.canvasy(event.y))
    _end = None

def _on_drop(event):
    global _start
    global _end
    global _img
    global crop_btn

    if _end is None:
        crop_btn.config(state="disabled")

    else:

        # Acotar límites de seleción a la imagen
        img_x, img_y = _img.size

        x0, y0 = _start
        x0 = img_x if x0 > img_x else 0 if x0 < 0 else x0
        y0 = img_y if y0 > img_y else 0 if y0 < 0 else y0 
        _start = (x0, y0)

        x1, y1 = _end
        x1 = img_x if x1 > img_x else 0 if x1 < 0 else x1
        y1 = img_y if y1 > img_y else 0 if y1 < 0 else y1       
        _end = (x1, y1)

        # Normalizado para obtener vertice superior izquierdo e inferior derecho
        if x0 > x1:
            if y0 < y1: # _start es el vértice superior derecho
                _start = (x1, y0)
                _end = (x0, y1)
            else:       # _start es el vértice inferior derecho
                _start, _end = _end, _start
        else:
            if y0 > y1:  # _start es el vértice inferior izquierdo
                _start = (x0, y1)
                _end = (x1, y0)

        crop_btn.config(state="normal")

    # Redibujar rectágulo
    _draw_rectangle()



def _on_drag(event):
    global _start
    global _end
    global canvas

    x0, y0 = _start
    ex, ey = canvas.canvasx(event.x), canvas.canvasy(event.y)
    _end = (ex, ey)
    _draw_rectangle()


def _draw_rectangle():
    global canvas
    global _end
    global _start

    canvas.delete("rectangle")

    if _end is None or _start is None:    
        return None

    x0, y0 = _start
    x1, y1 = _end

    canvas.create_rectangle(x0, y0, x1, y1, fill="#18c194",
                            width=1, stipple="gray50", tags='rectangle'
                            )

def _crop_image(name):
    global _img
    global _start
    global _end
    # Recortado de la imagen 
    #print(_start, _end)
    cropped = _img.crop(_start + _end)
    cropped.show()
    cropped.save(name)
    

def recortar(name):
    global root, canvas, crop_btn
    
    root = tk.Toplevel()
    root.title("Recortar imagen")

    # Frame contenedor con canvas y barras de desplazamiento
    frame = tk.Frame(root, bd=2, relief=tk.SUNKEN)
    xscrollbar = tk.Scrollbar(frame, orient=tk.HORIZONTAL)
    yscrollbar = tk.Scrollbar(frame)
    canvas = tk.Canvas(frame, bd=0,
                       xscrollcommand=xscrollbar.set, yscrollcommand=yscrollbar.set
                       )
    open_btn = tk.Button(root, text="Abrir imagen", command=lambda: _open_image(name))
    crop_btn = tk.Button(root, text="Recortar imagen", state="disabled", command=lambda: _crop_image(name))


    # Estructurando el widget
    root.grid_rowconfigure(0, weight=1)
    root.grid_columnconfigure(0, weight=1)
    root.grid_columnconfigure(1, weight=1)
    frame.grid_rowconfigure(0, weight=1)
    frame.grid_columnconfigure(0, weight=1)
    frame.grid(row=0, column=0, columnspan=2, sticky="nsew")
    xscrollbar.grid(row=1, column=0, columnspan=2, sticky="ew")
    yscrollbar.grid(row=0, column=1, sticky="ns")
    canvas.grid(row=0, column=0, sticky="nsew")
    open_btn.grid(row=2, column=0, sticky="ew")
    crop_btn.grid(row=2, column=1, sticky="nsew")

    xscrollbar.config(command=canvas.xview)
    yscrollbar.config(command=canvas.yview)


# Inicio del mainloop de la app
#root.mainloop()