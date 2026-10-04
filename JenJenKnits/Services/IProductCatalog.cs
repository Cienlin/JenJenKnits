using JenJenKnits.Models;

namespace JenJenKnits.Services;

public interface IProductCatalog
{
    /// <summary>所有有效商品：精選依 featured 升冪在前，其餘依 slug 排序。</summary>
    IReadOnlyList<Product> GetAll();

    /// <summary>featured 有值的商品，依 featured 升冪。</summary>
    IReadOnlyList<Product> GetFeatured();

    IReadOnlyList<Product> GetByCategory(string category);

    Product? GetBySlug(string slug);

    /// <summary>不重複的分類，依商品排列順序。</summary>
    IReadOnlyList<string> GetAllCategories();
}
