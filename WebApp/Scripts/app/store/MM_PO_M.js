Ext.define('WEBAPP.store.MM_PO_M', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MM_PO_M',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'PO_NO', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BD0002/GetAllM',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});