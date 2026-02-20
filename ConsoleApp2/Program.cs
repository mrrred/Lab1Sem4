using ConsoleApp2.Data;
using ConsoleApp2.Data.Abstractions;
using ConsoleApp2.Entities;
using ConsoleApp2.Menu;
using ConsoleApp2.MenuService;
using System;
using System.Collections.Generic;

namespace ConsoleApp21
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var productFsManager = new FSManager("products.prd", isProductFile: true);
            var productSerializer = new ProductSerializer(dataLength: 50);
            var productHeader = new FileHeader();
            var productNodeNavigator = new ProductNodeNavigator(productHeader);
            var productListManager = new LLManager<Product>(
                productFsManager,
                productSerializer,
                productNodeNavigator
            );
            IProductManaging productRepository = new ProductRepository(
                productFsManager,
                productSerializer,
                productListManager,
                productNodeNavigator
            );

            var specFsManager = new FSManager("specs.prs", isProductFile: false);
            var specSerializer = new SpecSerializer();
            var specHeader = new FileHeader();
            var specNodeNavigator = new SpecNodeNavigator(specHeader);
            var specListManager = new LLManager<Spec>(
                specFsManager,
                specSerializer,
                specNodeNavigator
            );
            ISpecManaging specRepository = new SpecRepository(
                specFsManager,
                specSerializer,
                specListManager,
                specNodeNavigator
            );

            IFileService fileService = new FileService(productRepository, specRepository);

            IMenu menu = new ConsolPSMenu(new List<IMenuItemStringeble>
            {
                new CreateMenuItem(fileService),
                new OpenMenuItem(fileService),
                new InputMenuItem(fileService),
                new DeleteMenuItem(fileService),
                new RestoreMenuItem(fileService),
                new TruncateMenuItem(fileService),
                new PrintMenuItem(fileService),
                new HelpMenuItem(fileService),
                new ExitMenuItem()
            });

            menu.Show();
        }
    }
}
