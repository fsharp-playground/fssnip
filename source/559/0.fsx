type F4PA = class
  member x.Item i = PA.FromFloat4ParallelArray(x, i)
end