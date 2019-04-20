
namespace MemoryCache_Simulator
{
	//-----------------------------------------------------------------------------//
	// Define os tipos de Políticas de Mapeamento.	
	public enum AssignmentPolicyType { Direto = 1, Associativo, PorConjunto };

    //-----------------------------------------------------------------------------//
    // Define os Algoritmos de Substituição.
    public enum ReplacementAlgorithmType { Fifo = 1, Lru, Lfu, Mru, Random };

	//-----------------------------------------------------------------------------//
	// Define o estado de acesso à memória cache. 
	public enum CacheStatus { Miss = 1, Hit }

	//-----------------------------------------------------------------------------//
	// Define um endereço de memória.
	public struct MemoryAddressCache
	{
		public int Row;
		public int Col;
	}

	//-----------------------------------------------------------------------------//
	// Define a configuração do simulador.
	public struct SettingApp
	{
		public int CacheSize;
		public string RamPagesFile;
		public AssignmentPolicyType Mapping;
		public ReplacementAlgorithmType Replace;
	}
}
