Ext.define('WEBAPP.store.CE0006INI', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CE0006',
    pageSize: 9999, //為了一次呈現
    remoteSort: true,
    sorters: [{ property: 'STORE_LOC', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CE0006/AllINIPDA',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});