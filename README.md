# HnWasm

Hacker News reader in .NET 10 Blazor WebAssembly with themes, swipe on mobile, no dependencies.

https://glipitch.github.io/hnwasm

The initial HTML explains the reader before WebAssembly starts, and the homepage keeps the same description after loading. This avoids an empty loading-only page when a crawler cannot finish loading stories. Search Console previously classified the homepage as a soft 404; Google must recrawl to reassess that status.

The page declares `https://glipitch.github.io/hnwasm/` as its canonical address. The published `sitemap.xml` lists that page for Search Console submission. A `robots.txt` under `/hnwasm/` would have no effect; crawling rules belong at the GitHub Pages host root. The absent host-level file currently permits crawling.
