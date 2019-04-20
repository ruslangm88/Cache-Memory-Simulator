using System;

namespace MemoryCache_Simulator
{
    //-------------------------------------------------------------------//
    //
    // Classe que representa uma abstração de uma memória cache.
    //
    //-------------------------------------------------------------------//
    public class MemoryCache
	{
		private int nPages;
		private int CacheSize;
		private int[][] spaceCache;
		private bool tabularMemory;
		
		//-----------------------------------------------------------------//
		// Constructor de clase.
		public MemoryCache(int size)
		{
			nPages = 0;
			CacheSize = size;
			tabularMemory = false;

			spaceCache = new int[CacheSize][];
			for (int i = 0; i < CacheSize; i++) spaceCache[i] = new int[1];
		}

        //-----------------------------------------------------------------//
        // Estabelece si a representação da memória cache se assume de 
        // forma tabular ou não. False: indica representação linear, True:
        // uma representação tabular (tabela).
        public void setTabularRepresentation (bool tabular)
		{
			tabularMemory = tabular;

			CacheSize = (!tabular) ? CacheSize : CacheSize / 2;

			if (!tabularMemory)
			{
				spaceCache = new int[CacheSize][];
				for (int i = 0; i < CacheSize; i++) spaceCache[i] = new int[1];
			}
			else
			{
				spaceCache = new int[CacheSize][];
				for (int i = 0; i < CacheSize; i++) spaceCache[i] = new int[2];
			}

			ClearCache();
		}

		//-----------------------------------------------------------------//
		// Verifica se a memória cache está cheia. Retorna True em caso
		// de que a memória cache está cheia, False em caso contrário.
		public bool IsMemoryFull()
		{
			return (nPages >= ((tabularMemory) ? CacheSize * 2: CacheSize));
		}

        //-----------------------------------------------------------------//
        // Determina se existe ou não na cache a página de memória passada
        // por parâmetro. Se não existe se lança um Miss, se existe se lança
        // então um Hit.
        public CacheStatus PageInCache (int page)
		{
			bool exist = false;

			for (int i = 0; i < CacheSize && !exist; i++)
			{
				if (spaceCache[i][0] == page) exist = true;
				if (tabularMemory) if (spaceCache[i][1] == page) exist = true;
			}

			return (exist) ? CacheStatus.Hit : CacheStatus.Miss;
		}

		//-----------------------------------------------------------------//
		// Busca uma página de memória na cache e retorna seu endereço.
		public MemoryAddressCache GetAddressForPage (int page)
		{
			bool exist = false;
			MemoryAddressCache address;

			address.Row = address.Col = 0;
			for (int i = 0; i < CacheSize && !exist; i++)
			{
				if (spaceCache[i][0] == page)
				{
					exist = true;
					address.Row = i;
					address.Col = 0;
				}
				if (tabularMemory) if (spaceCache[i][1] == page)
				{
					exist = true;
					address.Row = i;
					address.Col = 1;
				}
			}

			return address;
		}

		//-----------------------------------------------------------------//
		// Escreve na cache uma página de memória na posição indicada
		// por position.
		public void WritePage	(MemoryAddressCache position, int page)
		{
			if (spaceCache[position.Row][position.Col] == 0) nPages++;
			spaceCache[position.Row][position.Col] = page;
		}

		//-----------------------------------------------------------------//
		// Imprime o conteúdo da memória cache.
		public void PrintMemoryCache()
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("[ ");

			for (int i = 0; i < CacheSize; i++)
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.Write("{0,2}", (spaceCache[i][0] != 0) ? Convert.ToString(spaceCache[i][0]) : "");
				Console.ForegroundColor = ConsoleColor.Gray;
				if (i < CacheSize - 1) Console.Write(" | ");
			}
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(" ]\n{0}", (!tabularMemory) ? "\n" : "");

			if (tabularMemory)
			{
				Console.Write("\t\t\t\t     [ ");
				for (int i = 0; i < CacheSize; i++)
				{
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.Write("{0,2}", (spaceCache[i][1] != 0) ? Convert.ToString(spaceCache[i][1]) : "");
					Console.ForegroundColor = ConsoleColor.Gray;
					if (i < CacheSize - 1) Console.Write(" | ");
				}
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write(" ]\n\n");
			}
		}

		//-----------------------------------------------------------------//
		// Limpa toda a memória Cache, apagando todas as páginas de 
		// endereços que esta contenha.
		public void ClearCache ()
		{
			nPages = 0;
			if (!tabularMemory)
				for (int i = 0; i < CacheSize; i++)	spaceCache[i][0] = 0;
			else
				for (int i = 0; i < CacheSize; i++) spaceCache[i][0] = spaceCache[i][1] = 0;
		}
	}
}
