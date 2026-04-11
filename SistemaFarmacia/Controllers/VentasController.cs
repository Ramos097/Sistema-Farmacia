using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaFarmacia.Data;
using SistemaFarmacia.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaFarmacia.Controllers
{
    public class VentasController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                context.Result = RedirectToAction("Login", "Account");
            }
        }

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
            // ❌ Validar producto
            if (ProductoId == 0)
            {
                TempData["Error"] = "Debe seleccionar un producto";
                return RedirectToAction("NuevaVenta");
            }

            // ❌ Validar cantidad
            if (Cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a 0";
                return RedirectToAction("NuevaVenta");
            }

            var producto = await _context.Productos.FindAsync(ProductoId);

            if (producto == null)
            {
                TempData["Error"] = "Producto no existe";
                return RedirectToAction("NuevaVenta");
            }

            // 🔍 Buscar si ya existe en el carrito
            var itemExistente = carrito.FirstOrDefault(x => x.ProductoId == ProductoId);

            int cantidadTotal = Cantidad;

            if (itemExistente != null)
            {
                cantidadTotal += itemExistente.Cantidad;
            }

            // ❌ Validar stock total
            if (producto.Stock < cantidadTotal)
            {
                TempData["Error"] = $"Stock insuficiente. Disponible: {producto.Stock}";
                return RedirectToAction("NuevaVenta");
            }

            if (itemExistente != null)
            {
                itemExistente.Cantidad += Cantidad;
            }
            else
            {
                var item = new ItemVenta
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Cantidad = Cantidad,
                    Precio = producto.PrecioVenta
                };

                carrito.Add(item);
            }

            TempData["Success"] = "Producto agregado al carrito";

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
