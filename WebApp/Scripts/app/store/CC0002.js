Ext.define('WEBAPP.store.CC0002', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MM_PO_M',
    pageSize: 1000,
    remoteSort: true,
    sorters: [{ property: 'M_CONTID', direction: 'ASC' }, { property: 'PO_NO', direction: 'DESC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CC0002/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});