using System;

namespace MemoryCache_Simulator
{
    //-------------------------------------------------------------------//
    //
    //	Classe Abstrata Pai que representa um Algoritmo de Substituição
    //	(Substitution Algorithm) de memória Cache.
    //
    //-------------------------------------------------------------------//
    public abstract class ReplacementAlgorithm
	{
		protected int CacheSize;
		protected bool tabularMemory;
		protected MemoryAddressCache Address;

		public abstract MemoryAddressCache Calculate();
		public abstract void FeedBack(MemoryAddressCache add);

		//-----------------------------------------------------------------//
		// Construtor da clase base.
		public ReplacementAlgorithm (int cache_size)
		{
			CacheSize = (tabularMemory) ? cache_size / 2 : cache_size;
			Address = new MemoryAddressCache();
		}

		//-----------------------------------------------------------------//
		// Método Virtual. Estabelece si a representação da memória 
		// cache se assume de forma tabular ou no. False: indica representação 
		// linear, True: uma representação tabular (tabela).
		public virtual void setTabularRepresentation (bool tabular)
		{
			tabularMemory = tabular;
			CacheSize = (tabularMemory) ? CacheSize / 2 : CacheSize;
		}
	}

    //-------------------------------------------------------------------//
    //
    // Classe Concreta que representa um Algoritmo de Substituição FIFO.
    //	 FIFO: First In First Out
    //
    //	Nota: esta política descarta primeiro os elementos que foram 
    //			  os primeiros em escreverse na memória cache. Se implementa
    //				como uma fila.
    //
    //-------------------------------------------------------------------//
    public class RA_FIFO : ReplacementAlgorithm
	{
		private int age;
		private int[][] fifo;

		//-----------------------------------------------------------------//
		// Construtor de clase.
		public RA_FIFO(int cache_size) : base(cache_size)
		{
			age = 0;
			fifo = new int[CacheSize][];
			for (int i = 0; i < CacheSize; i++) fifo[i] = new int[1];
		}

		//-----------------------------------------------------------------//
		// Calcula a posição de memória da cache que debe ser substituída
		// para uma política FIFO.
		public override MemoryAddressCache Calculate()
		{
			int min, indexR, indexC;

			indexR = 0;
			indexC = 0;
			min = fifo[0][0];
			for (int i = 1; i < CacheSize; i++)
			{
				if (fifo[i][0] < min)
				{
					min = fifo[i][0];
					indexR = i;
				}

				if (tabularMemory && fifo[i][1] < min)
				{
					min = fifo[i][1];
					indexC = 1;
				}
			}

			Address.Row = indexR;
			Address.Col = indexC;

			return Address;
		}

		//-----------------------------------------------------------------//
		// Método Virtual Sobrescrito. Estabelece si a representação da 
		// memória cache se assume de forma tabular ou não. False: indica 
		// representação linear, True: uma representação tabular (tabela).
		public override void setTabularRepresentation(bool tabular)
		{
			int col;

			tabularMemory = tabular;
			CacheSize = (tabularMemory) ? CacheSize / 2 : CacheSize;
			col = (tabularMemory) ? 2 : 1;
			fifo = new int[CacheSize][];

			for (int i = 0; i < CacheSize; i++) fifo[i] = new int[col];
		}

        //-----------------------------------------------------------------//
        // Função para informar à Política de Substituição sobre as mudanças
        // que aconteceram na memória cache durante a última operação 
        // de escritura.
        public override void FeedBack(MemoryAddressCache add)
		{
			fifo[add.Row][add.Col] = age++;
		}
	}

    //-------------------------------------------------------------------//
    //
    //	Classe concreta que representa um Algoritmo de Substituição LFU
    //		LFU: Least Frecuently Used.
    //
    //	Nota: esta política descarta primeiro aqueles elementos menos
    //			  usados recentemente, esta métrica se calcula a partir 
    //				da frequência de uso dos endereços na memória
    //				cache.
    //
    //-------------------------------------------------------------------//
    public class RA_LFU : ReplacementAlgorithm
	{
		private int[][] lfu;

		//-----------------------------------------------------------------//
		// Construtor clase.
		public RA_LFU (int cache_size) : base(cache_size)
		{
			lfu = new int[CacheSize][];
			for (int i = 0; i < CacheSize; i++)
			{
				lfu[i] = new int[1];
				lfu[i][0] = 0;
			}
		}

		//-----------------------------------------------------------------//
		// Calcula a posição de memória da cache que deve ser substituída
		// para uma política LFU.
		public override MemoryAddressCache Calculate()
		{
			int min, indexR, indexC;

			indexR = 0;
			indexC = 0;
			min = lfu[0][0];
			for (int i = 1; i < CacheSize; i++)
			{
				if (lfu[i][0] < min)
				{
					min = lfu[i][0];
					indexR = i;
				}

				if (tabularMemory && lfu[i][1] < min)
				{
					min = lfu[i][1];
					indexC = 1;
				}
			}

			lfu[indexR][indexC] = 0;
			Address.Row = indexR;
			Address.Col = indexC;

			return Address;
		}

		//-----------------------------------------------------------------//
		// Método Virtual Sobrescrito. Estabelece se a representação da 
		// memória cache se assume de forma tabular ou não. False: indica 
		// representação linear, True: uma representação tabular (tabela).
		public override void setTabularRepresentation(bool tabular)
		{
			int col;

			tabularMemory = tabular;
			CacheSize = (tabularMemory) ? CacheSize / 2 : CacheSize;
			col = (tabularMemory) ? 2 : 1;
			lfu = new int[CacheSize][];

			for (int i = 0; i < CacheSize; i++)
			{ 
				lfu[i] = new int[col];
				for (int j = 0; j < col; j++) lfu[i][j] = 0;
			}
		}

        //-----------------------------------------------------------------//
        // Função para informar à Política de Substituição sobre as mudanças
        // que aconteceram na memória cache durante a última operação 
        // de escritura.
        public override void FeedBack (MemoryAddressCache add)
		{
			lfu[add.Row][add.Col]++;
		}
	}

    //-------------------------------------------------------------------//
    //
    // Classe Concreta que representa um Algoritmo de Substituição LRU.
    //	 LRU: Least Recently Used.
    //
    //	Nota: esta política descarta primeiro os elementos menos usados
    //			  recentemente, ou seja, substitui os endereços de memória
    //			  que faz mais tempo que não se utilizam.	
    //
    //-------------------------------------------------------------------//
    public class RA_LRU : ReplacementAlgorithm
	{
		private int time;
		private int[][] lru;

		//-----------------------------------------------------------------//
		// Constructor de clase.
		public RA_LRU (int cache_size) : base(cache_size)
		{
			time = 0;
			lru = new int[CacheSize][];
			for (int i = 0; i < CacheSize; i++) lru[i] = new int[1];
		}

		//-----------------------------------------------------------------//
		// Calcula a posição de memória da cache que deve ser substituída
		// para uma política LRU.
		public override MemoryAddressCache Calculate()
		{
			int min, indexR, indexC;

			indexR = 0;
			indexC = 0;
			min = lru[0][0];
			for (int i = 1; i < CacheSize; i++)
			{
				if (lru[i][0] < min)
				{
					min = lru[i][0];
					indexR = i;
				}

				if (tabularMemory && lru[i][1] < min)
				{
					min = lru[i][1];
					indexC = 1;
				}
			}

			lru[indexR][indexC] = 0;
			Address.Row = indexR;
			Address.Col = indexC;

			return Address;
		}

		//-----------------------------------------------------------------//
		// Método Virtual Sobrescrito. Estabelece se a representação da 
		// memória cache se assume de forma tabular ou não. False: indica 
		// representação linear, True: uma representação tabular (tabela).
		public override void setTabularRepresentation(bool tabular)
		{
			int col;

			tabularMemory = tabular;
			CacheSize = (tabularMemory) ? CacheSize / 2 : CacheSize;
			col = (tabularMemory) ? 2 : 1;
			lru = new int[CacheSize][];

			for (int i = 0; i < CacheSize; i++)
			{
				lru[i] = new int[col];
				for (int j = 0; j < col; j++) lru[i][j] = 0;
			}
		}

        //-----------------------------------------------------------------//
        // Função para informar à Política de Substituição sobre as mudanças
        // que aconteceram na memória cache durante a última operação 
        // de escritura.
        public override void FeedBack(MemoryAddressCache add)
		{
			lru[add.Row][add.Col] = time++;
		}
	}

    //-------------------------------------------------------------------//
    //
    // Classe Concreta que representa um Algoritmo de Substituição MRU.
    //	 MRU: Most Recently Used.
    //
    //	Nota: esta política descarta primeiro (ao contrário do LRU)
    //				aqueles elementos mais usados recentemente, substitui 
    //			  os endereços de memória que faz menos tempo que não se 
    //				utilizam.
    //
    //-------------------------------------------------------------------//
    public class RA_MRU : ReplacementAlgorithm
	{
		private int time;
		private int[][] mru;

		//-----------------------------------------------------------------//
		//
		public RA_MRU(int cache_size) : base(cache_size)
		{
			time = 0;
			mru = new int[CacheSize][];
			for (int i = 0; i < CacheSize; i++) mru[i] = new int[1];
		}

		//-----------------------------------------------------------------//
		// Calcula a posição de memória da cache que deve ser substituída
		// para uma política MRU
		public override MemoryAddressCache Calculate()
		{
			int max, indexR, indexC;

			indexR = 0;
			indexC = 0;
			max = mru[0][0];
			for (int i = 1; i < CacheSize; i++)
			{
				if (mru[i][0] > max)
				{
					max = mru[i][0];
					indexR = i;
				}

				if (tabularMemory && mru[i][1] > max)
				{
					max = mru[i][1];
					indexC = 1;
				}
			}

			mru[indexR][indexC] = 0;
			Address.Row = indexR;
			Address.Col = indexC;

			return Address;
		}

		//-----------------------------------------------------------------//
		// Método Virtual Sobrescrito. Estabelece se a representação da 
		// memória cache se assume de forma tabular ou não. False: indica 
		// representação linear, True: uma representação tabular (tabela).
		public override void setTabularRepresentation(bool tabular)
		{
			int col;

			tabularMemory = tabular;
			CacheSize = (tabularMemory) ? CacheSize / 2 : CacheSize;
			col = (tabularMemory) ? 2 : 1;
			mru = new int[CacheSize][];

			for (int i = 0; i < CacheSize; i++)
			{
				mru[i] = new int[col];
				for (int j = 0; j < col; j++) mru[i][j] = 0;
			}
		}

        //-----------------------------------------------------------------//
        // Função para informar à Política de Substituição sobre as mudanças
        // que aconteceram na memória cache durante a última operação 
        // de escritura.
        public override void FeedBack(MemoryAddressCache add)
		{
			mru[add.Row][add.Col] = time++;
		}
	}

	//-------------------------------------------------------------------//
	//
	// Classe Concreta que representa um Algoritmo de Substituição RANDOM. 
	//
	//	Nota: esta política não requer salvar informação sobre o 
	//			  histórico de acesso à memória cache. Os elementos a 
	//			  descartar são selecionados aleatoriamente.
	//
	//-------------------------------------------------------------------//
	public class RA_RANDOM : ReplacementAlgorithm
	{
		public RA_RANDOM (int cache_size) : base(cache_size) { }

		//-----------------------------------------------------------------//
		// Calcula 
		public override MemoryAddressCache Calculate()
		{
			Random rnd = new Random((int)DateTime.Now.Ticks);

			Address.Row = rnd.Next() % CacheSize;
			Address.Col =	(!tabularMemory) ? 0 : rnd.Next() % 2;

			return Address;
		}

        //-----------------------------------------------------------------//
        // Função para informar à Política de Substituição sobre as mudanças
        // que aconteceram na memória cache durante a última operação 
        // de escritura.
        public override void FeedBack (MemoryAddressCache add) 
		{
			throw new NotImplementedException("O algoritmo de substituição RANDOM não precisa fazer feedback de informação!!!");
		}
	}
}
