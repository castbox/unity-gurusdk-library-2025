using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;

namespace Guru
{
	public static partial class FirebaseUtil
	{
		private static FirebaseFirestore _firestore => FirebaseFirestore.DefaultInstance;
        
        #region 添加数据
        
        public static void FireStore_SetDocument(string collection, object data)
        {
            _firestore.Collection(collection)?.AddAsync(data);
        }
        
        public static void FireStore_SetData(string collection, string document, object data, SetOptions options)
        {
            _firestore.Collection(collection).Document(document).SetAsync(data, options);
        }

        public static void FireStore_SetData(string collection, string document, Dictionary<string, object> data, SetOptions options)
        {
            _firestore.Collection(collection).Document(document).SetAsync(data, options);
        }
        
        public static void FireStore_UpdateData(string collection, string document, IDictionary<string, object> data)
        {
            _firestore.Collection(collection).Document(document).UpdateAsync(data);
        }
        
        public static void FireStore_UpdateData(string collection, string document, string field, object data)
        {
            _firestore.Collection(collection).Document(document).UpdateAsync(field, data);
        }
        
        #endregion
        
        #region 自定义类形式获取数据
        
        public static void FireStore_GetDocumentData<T>(string collection, string document,
            Action<T> SuccessCallBack, Action<string> FailCallBack)
        {
            try
            {
                _firestore.Collection(collection).Document(document).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            FailCallBack?.Invoke(task?.Exception?.ToString());
                            Log.E(LOG_TAG, task.Exception);
                        }
                        else
                        {
                            DocumentSnapshot documentSnapshot = task.Result;
                            if (documentSnapshot != null && documentSnapshot.Exists)
                            {
                                T t = documentSnapshot.ConvertTo<T>();
                                if (t != null)
                                {
                                    SuccessCallBack?.Invoke(t);
                                }
                                else
                                {
                                    FailCallBack?.Invoke("conver to is null");
                                }
                            }
                            else
                            {
                                FailCallBack?.Invoke("no document exist");
                            }
                        }
                    });
            }
            catch (Exception e)
            {
                FailCallBack.Invoke(e.ToString());
                Log.E(LOG_TAG, e.ToString());
            }
        }
        
        public static void FireStore_GetDocumentDataAsync<T>(string collection, string document, 
            Action<T> SuccessCallBack, Action<string> FailCallBack)
        {
            try
            {
                _firestore.Collection(collection).Document(document).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            FailCallBack?.Invoke(task?.Exception?.ToString());
                            Log.E(LOG_TAG, task.Exception);
                        }
                        else
                        {
                            DocumentSnapshot documentSnapshot = task.Result;
                            if (documentSnapshot != null && documentSnapshot.Exists)
                            {
                                T t = documentSnapshot.ConvertTo<T>();
                                if (t != null)
                                {
                                    SuccessCallBack?.Invoke(t);
                                }
                                else
                                {
                                    FailCallBack?.Invoke("conver to is null");
                                }
                            }
                            else
                            {
                                FailCallBack?.Invoke("no document exist");
                            }
                        }
                    });
            }
            catch (Exception e)
            {
                FailCallBack.Invoke(e.ToString());
                Log.E(LOG_TAG, e.ToString());
            }
        }
        
        #endregion
        
        #region Dictionary<string, object>形式获取数据
        
        public static void FireStore_GetDocumentData(string collection, string document, 
            Action<Dictionary<string, object>> SuccessCallBack, Action<string> FailCallBack)
        {
            try
            {
                _firestore.Collection(collection).Document(document).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            FailCallBack?.Invoke(task?.Exception?.ToString());
                            Log.E(LOG_TAG, task.Exception);
                        }
                        else
                        {
                            DocumentSnapshot documentSnapshot = task.Result;
                            if (documentSnapshot != null && documentSnapshot.Exists)
                            {
                                SuccessCallBack?.Invoke(documentSnapshot.ToDictionary());
                            }
                            else
                            {
                                FailCallBack?.Invoke("no document exist");
                            }
                        }
                    });
            }
            catch (Exception e)
            {
                FailCallBack.Invoke(e.ToString());
                Log.E(LOG_TAG, e.ToString());
            }
        }
        
        public static void FireStore_GetDocumentDataAsync(string collection, string document, Action<Dictionary<string, object>> SuccessCallBack, Action<string> FailCallBack)
        {
            try
            {
                _firestore.Collection(collection).Document(document).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            FailCallBack?.Invoke(task?.Exception?.ToString());
                            Log.E(LOG_TAG, task.Exception);
                        }
                        else
                        {
                            DocumentSnapshot documentSnapshot = task.Result;
                            if (documentSnapshot != null && documentSnapshot.Exists)
                            {
                                SuccessCallBack?.Invoke(documentSnapshot.ToDictionary());
                            }
                            else
                            {
                                FailCallBack?.Invoke("no document exist");
                            }
                        }
                    });
            }
            catch (Exception e)
            {
                FailCallBack.Invoke(e.ToString());
                Log.E(LOG_TAG, e.ToString());
            }
        }

        #endregion

        #region 获取Collections下所有文档
        
        public static void FireStore_GetCollections<T>(string collection, Action<List<T>> SuccessCallBack, Action<string> FailCallBack)
        {
            try
            {
                _firestore.Collection(collection).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            FailCallBack?.Invoke(task?.Exception?.ToString());
                            Log.E(LOG_TAG, task.Exception);
                        }
                        else
                        {
                            List<T> results = new List<T>();
                            foreach (DocumentSnapshot documentSnapshot in task.Result.Documents)
                            {
                                results.Add(documentSnapshot.ConvertTo<T>());
                            }
                            SuccessCallBack?.Invoke(results);
                        }
                    });
            }
            catch (Exception e)
            {
                FailCallBack.Invoke(e.ToString());
                Log.E(LOG_TAG, e.ToString());
            }
        }

        public static void FireStore_GetCollectionsAsync<T>(string collection, Action<List<T>> SuccessCallBack, Action<string> FailCallBack)
        {
            try
            {
                _firestore.Collection(collection).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            FailCallBack?.Invoke(task?.Exception?.ToString());
                            Log.E(LOG_TAG, task.Exception);
                        }
                        else
                        {
                            List<T> results = new List<T>();
                            foreach (DocumentSnapshot documentSnapshot in task.Result.Documents)
                            {
                                results.Add(documentSnapshot.ConvertTo<T>());
                            }
                            SuccessCallBack?.Invoke(results);
                        }
                    });
            }
            catch (Exception e)
            {
                FailCallBack.Invoke(e.ToString());
                Log.E(LOG_TAG, e.ToString());
            }
        }
        
        #endregion
	}
}