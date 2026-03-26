using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaFarmacia.Data;
using SistemaFarmacia.Models;

namespace SistemaFarmacia.Controllers
{
    public class VentasController : Controller
    {
        private readonly FarmaciaContext _context;
        private static List<ItemVenta> carrito = new List<ItemVenta>();


        public VentasController(FarmaciaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CrearVenta()
        {
            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Total = 0
            };

            // Ejemplo: agregar productos manualmente
            var producto = await _context.Productos.FirstOrDefaultAsync();

            if (producto != null && producto.Stock > 0)
            {
                int cantidad = 2;

                var detalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = cantidad,
                    Precio = producto.PrecioVenta,
                    Subtotal = producto.PrecioVenta * cantidad
                };

                venta.Detalles.Add(detalle);

                // Descontar stock
                producto.Stock -= cantidad;

                // Calcular total
                venta.Total += detalle.Subtotal;
            }

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return Content("Venta creada correctamente");
        }
        public IActionResult NuevaVenta()
        {
            ViewData["Productos"] = new SelectList(_context.Productos, "Id", "Nombre");
            ViewBag.Carrito = carrito;
            ViewBag.Total = carrito.Sum(x => x.Subtotal);
            return View();
        }
       
        [HttpPost]
        public async Task<IActionResult> GuardarVenta(int ProductoId, int Cantidad)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(ProductoId);

                if (producto == null || producto.Stock < Cantidad)
                {
                    return Content("Stock insuficiente o producto no existe");
                }

                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    Total = 0
                };

                var detalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = Cantidad,
                    Precio = producto.PrecioVenta,
                    Subtotal = producto.PrecioVenta * Cantidad
                };

                venta.Detalles.Add(detalle);

                producto.Stock -= Cantidad;

                venta.Total = detalle.Subtotal;

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                return Content("Venta realizada correctamente");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AgregarProducto(int ProductoId, int Cantidad)
        {
            var producto = await _context.Productos.FindAsync(ProductoId);

            if (producto == null || producto.Stock < Cantidad)
            {
                TempData["Error"] = "No hay suficiente stock de este producto";
                return RedirectToAction("NuevaVenta");
            }

            // 🔍 Buscar si ya existe en el carrito
            var itemExistente = carrito.FirstOrDefault(x => x.ProductoId == ProductoId);

            if (itemExistente != null)
            {
                // Validar stock total
                if (producto.Stock < itemExistente.Cantidad + Cantidad)
                {
                    return Content("Stock insuficiente para esa cantidad total");
                }

                // Sumar cantidad
                itemExistente.Cantidad += Cantidad;
            }
            else
            {
                // Crear nuevo item
                var item = new ItemVenta
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Cantidad = Cantidad,
                    Precio = producto.PrecioVenta
                };

                carrito.Add(item);
            }

            return RedirectToAction("NuevaVenta");
        }
        [HttpPost]
        public async Task<IActionResult> FinalizarVenta()
        {
            if (carrito.Count == 0)
            {
                return Content("No hay productos en el carrito");
            }

            var venta = new Venta
            {
                Fecha = DateTime.Now
            };

            decimal total = 0;

            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);

                if (producto == null || producto.Stock < item.Cantidad)
                {
                    return Content($"Stock insuficiente para {item.Nombre}");
                }

                var detalle = new DetalleVenta
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    Precio = item.Precio,
                    Subtotal = item.Subtotal
                };

                producto.Stock -= item.Cantidad;

                total += item.Subtotal;

                venta.Detalles.Add(detalle);
            }

            venta.Total = total;

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            TempData["Detalle"] = System.Text.Json.JsonSerializer.Serialize(carrito);

            carrito.Clear();

            TempData["Total"] = total.ToString("N2");
            TempData["Mensaje"] = "Venta realizada correctamente";

            return RedirectToAction("Factura");
        }
        [HttpPost]
        public IActionResult EliminarProducto(int index)
        {
            if (index >= 0 && index < carrito.Count)
            {
                carrito.RemoveAt(index);
            }

            return RedirectToAction("NuevaVenta");
        }
        [HttpPost]
        public IActionResult EditarCantidad(int index, int nuevaCantidad)
        {
            if (index >= 0 && index < carrito.Count)
            {
                if (nuevaCantidad <= 0)
                {
                    carrito.RemoveAt(index);
                }
                else
                {
                    carrito[index].Cantidad = nuevaCantidad;
                }
            }

            return RedirectToAction("NuevaVenta");
        }
        public IActionResult Factura()
        {
            ViewBag.Total = TempData["Total"];
            ViewBag.Mensaje = TempData["Mensaje"];

            var detalleJson = TempData["Detalle"] as string;

            if (detalleJson != null)
            {
                ViewBag.Detalle = System.Text.Json.JsonSerializer
                    .Deserialize<List<ItemVenta>>(detalleJson);
            }

            return View();
        }
    }
}
