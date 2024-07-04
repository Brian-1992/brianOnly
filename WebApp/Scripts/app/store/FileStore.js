//定義FileStore資料儲存所
Ext.define('WEBAPP.store.FileStore', {
    extend: 'Ext.data.Store',
    requires: ['WEBAPP.model.FileModel'],
    model: 'WEBAPP.model.FileModel',
    autoLoad: false,
    sorters: [{ property: 'FC', direction: 'DESC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST'
        },
        url: '/api/File/GetByKey',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});