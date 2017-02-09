// Graph Engine
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//
using FanoutSearch.Protocols.TSL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Trinity.Daemon;
using Trinity.Diagnostics;

namespace FanoutSearch
{
    internal class QueryResultCacheEntry
    {
        public AggregationObject aggregation_obj;
        public int transaction_id;

        public QueryResultCacheEntry(int trans, AggregationObject aggr)
        {
            aggregation_obj = aggr;
            transaction_id  = trans;
        }
    }

    internal class QueryResultCache
    {
        private QueryPredicateCompiler m_compiler;
        private MemoryCache            m_memory_cache;
        private CacheItemPolicy        m_cache_policy;

        internal QueryResultCache(QueryPredicateCompiler compiler)
        {
            m_compiler = compiler;
            m_memory_cache = MemoryCache.Default;
            m_cache_policy = new CacheItemPolicy();
            m_cache_policy.SlidingExpiration = compiler.GetExperationTime();
        }

        internal void RegisterQueryResult(int transaction_id, FanoutQueryMessageReader request, AggregationObject aggregation_obj)
        {
            if(aggregation_obj.results.Count == 0)
            {
                Log.WriteLine(LogLevel.Debug, "QueryResultCache: ignoring empty query result, transaction id = {0}.", transaction_id);
                return;
            }

            QueryResultCacheEntry entry = new QueryResultCacheEntry(transaction_id, aggregation_obj);
            string key_query = GetQueryResultCacheRequestKey(request);
            string key_trans = GetQueryResultCacheTransactionKey(entry.transaction_id);

            m_memory_cache.Set(key_query, entry, m_cache_policy);
            m_memory_cache.Set(key_trans, entry, m_cache_policy);
        }

        /// <returns>null if no cached query result is found.</returns>
        internal AggregationObject GetCachedQueryResult(FanoutQueryMessageReader query)
        {
            string key                  = GetQueryResultCacheRequestKey(query);
            QueryResultCacheEntry entry = (QueryResultCacheEntry)m_memory_cache.Get(key);

            if (entry == null) return null;
            int result_cnt = entry.aggregation_obj.results.Count;
            if (result_cnt < FanoutSearchModule.MinimalRequiredResultCount(query)) return null;

            Log.WriteLine(LogLevel.Debug, "QueryResultCache: Cache hit.");

            return entry.aggregation_obj;
        }

        private string GetQueryResultCacheTransactionKey(int transaction_id)
        {
            return "transaction:" + transaction_id;
        }

        private string GetQueryResultCacheRequestKey(FanoutQueryMessageReader request)
        {
            StringBuilder sb = new StringBuilder("request:");
            sb.Append(Serializer.ToString(request.origin));
            sb.Append(':');
            sb.Append(request.originQuery);
            sb.Append(':');
            sb.Append(Serializer.ToString(request.predicates));
            sb.Append(':');
            sb.Append(Serializer.ToString(request.edge_types));
            return sb.ToString();
        }
    }
}
