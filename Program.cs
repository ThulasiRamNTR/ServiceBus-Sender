using Azure.Messaging.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace QueueSender
{
    internal class Program
    {
        private static readonly String connectionString = "";
        private static readonly String queueName = "";
        private static ServiceBusClient client;
        private static ServiceBusSender sender;
        private const Int32 numOfMessages = 3;

        public static void Main()
        {
            SendMessageAsync().GetAwaiter().GetResult();
            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }

        private static async Task SendMessageAsync()
        {


            // create a batch 
            try
            {
                client = new ServiceBusClient(connectionString);
                sender = client.CreateSender(queueName);

                Int32 counter = 0;

                do
                {
                    using (ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync())
                    {
                        AddMessageContent(counter, messageBatch);
                        counter++;
                        await sender.SendMessagesAsync(messageBatch);
                        Console.WriteLine("Message sent successfully!");
                        if (counter == 50) { break; }

                    }
                } while (true);
            }
            catch (ServiceBusException ex)
            {
                Type mainType = ex.GetType();
                Type type = ex.InnerException.GetType();
                Console.Clear();
            }
            catch (UnauthorizedAccessException ex)
            {
                Type type = ex.InnerException.GetType();
                Console.Clear();
            }
            catch (Exception ex)
            {
                Type type = ex.GetType();
                Console.Clear();
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
        /// <summary>
        /// Helps to get the content to be added into the ServiceBusQueue
        /// Types of content:
        /// 1. {randomguid}-SuccessMessage
        /// 2. {randomguid}-ErrorQueueMessage
        /// 3. {randomguid}-ErrorTimeOutMessage
        /// 4. 
        /// 
        /// </summary>
        /// <param name="counter">Counter</param>
        /// <returns>get the current message according to the counter</returns>
        private static void AddMessageContent(Int32 counter, ServiceBusMessageBatch messageBatch)
        {
            StringBuilder stringBuilderUtil = new StringBuilder();
            stringBuilderUtil.Append(Guid.NewGuid().ToString());
            if (counter % 13 == 0)
            {
                stringBuilderUtil.Append("-ErrorTimeOutMesage");
                AddContent(messageBatch, stringBuilderUtil.ToString());
            }
            else if (counter % 7 == 0)
            {
                stringBuilderUtil.Append("-ErrorQueueMessage");
                AddContent(messageBatch, stringBuilderUtil.ToString());
            }
            else
            {
                stringBuilderUtil.Append("-SuccessMessage");
                AddContent(messageBatch, stringBuilderUtil.ToString());
            }
        }

        /// <summary>
        /// Helps to add the given content into the ServiceBusMessagebatch
        /// </summary>
        /// <param name="messageBatch">ServiceBusMessageBatch</param>
        /// <param name="content">Content to be added</param>
        private static void AddContent(ServiceBusMessageBatch messageBatch, String content)
        {
            Console.WriteLine(content);
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(content)))
            {
                // if it is too large for the batch
                throw new Exception($"The message is too large to fit in the batch.");
            }
        }
    }
}