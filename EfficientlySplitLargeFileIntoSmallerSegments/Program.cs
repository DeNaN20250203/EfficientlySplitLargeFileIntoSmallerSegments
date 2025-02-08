namespace EfficientlySplitLargeFileIntoSmallerSegments
{
    internal class Program
    {
        static void Main(string[] args)
        {
		string inputFile = @"C:\Work\Загрузки\example.txt";
		string outputTemplate = @"C:\Work\Загрузки\output_{0}.txt";
		//long maxChunkSize = 10L * 1024 * 1024 * 1024; // 10 ГБ
		long maxChunkSize = 100; // Размер чанка в байтах Пример для теста программы

		SplitFile(inputFile, outputTemplate, maxChunkSize);
		Console.WriteLine("Готово! :)");
		Console.ReadKey();
	}

	/// <summary>
	/// <para>Method…</para>
	/// <para>Разделяет большой файл на несколько меньших файлов, 
	/// основываясь на максимальном размере чанка.</para>
	/// </summary>
	/// <param name="inputPath">Путь к исходному файлу, который нужно разделить.</param>
	/// <param name="outputPathTemplate">Шаблон для именования выходных файлов. 
	/// Используйте {0} для номера части.</param>
	/// <param name="maxChunkSize">Максимальный размер чанка в байтах.</param>
	static void SplitFile(string inputPath, string outputPathTemplate, long maxChunkSize)
	{
		const int bufferSize = 4096 * 1024; // 4 МБ буфер для чтения
		byte[] buffer = new byte[bufferSize];
		List<byte> lineBuffer = new List<byte>();
		int partNumber = 1;
		long currentChunkSize = 0;
		FileStream? currentOutputStream = null;

		try
		{
			using (var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
			{
				while (true)
				{
					int bytesRead = inputStream.Read(buffer, 0, buffer.Length);
					if (bytesRead == 0) break;
					for (int i = 0; i < bytesRead; i++)
					{
						byte b = buffer[i];
						lineBuffer.Add(b);

						// Определение конца строки (поддерживает \n и \r\n)
						if (b == '\n')
						{
							ProcessLine(ref lineBuffer, ref currentOutputStream!,
								  ref partNumber, ref currentChunkSize,
								  maxChunkSize, outputPathTemplate);
						}
					}
				}

				// Обработка последней строки без \n
				if (lineBuffer.Count > 0)
				{
					ProcessLine(ref lineBuffer, ref currentOutputStream!,
							  ref partNumber, ref currentChunkSize,
							  maxChunkSize, outputPathTemplate);
					}
				}
			}
			finally
			{
				currentOutputStream?.Dispose();
			}
		}


		/// <summary>
		/// <para>Method…</para>
		/// <para>Обрабатывает строку, записывая её в текущий выходной поток 
		/// или создавая новый поток при необходимости.</para>
		/// </summary>
		/// <param name="lineBuffer">Буфер, содержащий данные строки.</param>
		/// <param name="currentOutputStream">Текущий выходной поток для записи данных.</param>
		/// <param name="partNumber">Номер части текущего файла.</param>
		/// <param name="currentChunkSize">Текущий размер чанка в байтах.</param>
		/// <param name="maxChunkSize">Максимальный размер чанка в байтах.</param>
		/// <param name="outputTemplate">Шаблон для именования выходных файлов.</param>
		static void ProcessLine(ref List<byte> lineBuffer, ref FileStream currentOutputStream,
							  ref int partNumber, ref long currentChunkSize,
							  long maxChunkSize, string outputTemplate)
		{
			byte[] lineBytes = lineBuffer.ToArray();
			lineBuffer.Clear();

			if (currentOutputStream == null || currentChunkSize + lineBytes.Length > maxChunkSize)
			{
				currentOutputStream?.Dispose();
				string newFilePath = string.Format(outputTemplate, partNumber++);
				currentOutputStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write);
				currentChunkSize = 0;
			}

			currentOutputStream.Write(lineBytes, 0, lineBytes.Length);
			currentChunkSize += lineBytes.Length;
		}
	}
}
