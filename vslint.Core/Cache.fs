
module vslint.Cache

/// <summary>
/// Function memoization
/// </summary>
/// <remarks>
/// Implementation from http://mugi.or.id/blogs/eriawan/archive/2013/09/27/implementations-of-memoization-in-f.aspx
/// </remarks>
/// <param name="f">Function to memoize</param>
let memoize f =
    let cache = ref Map.empty
    fun x ->
        match (!cache).TryFind(x) with
        | Some res -> res
        | None ->
            let res = f x
            cache := (!cache).Add(x,res)
            res
