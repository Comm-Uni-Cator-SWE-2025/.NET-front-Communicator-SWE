/*
 * -----------------------------------------------------------------------------
 *  File: IRPC.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Communicator.Core.RPC
{
    /**
     * This is the .NET interface contract for your RPC system.
     */
    public interface IRPC
    {
        /**
         * Registers a procedure on the server.
         * @param methodName The string name of the procedure.
         * @param method The function to execute.
         */
        void Subscribe(string methodName, Func<byte[], byte[]> method);

        /**
         * Connects the RPC server and starts its listener thread.
         * @param portNumber The port to listen on.
         * @return The thread that the server is running on.
         */
        Thread Connect(int portNumber); // IOException, etc. are not declared in C# interfaces

        /**
         * Calls a remote procedure on the server.
         * @param methodName The string name of the procedure to call.
         * @param data The byte array argument.
         * @return A Task (like a Future) that will eventually contain the byte[] response.
         */
        Task<byte[]> Call(string methodName, byte[] data);
    }
}
