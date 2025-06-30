using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Models;

namespace OXF.Pages
{
    public class ProdutosModel : PageModel
    {
        public List<Product> ListaDeProdutos { get; set; }

        public void OnGet()
        {
            // Simulação de dados (poderia vir de banco de dados)
            ListaDeProdutos = new List<Product>
            {
                new Product { Id = 1, Nome = "Prato", Preco = 79.90M },
                new Product { Id = 2, Nome = "Prato Fundo", Preco = 119.90M },
                new Product { Id = 3, Nome = "Prato Raso", Preco = 199.90M }
            };
        }
    }
}
