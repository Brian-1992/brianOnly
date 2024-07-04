Ext.define('WEBAPP.store.PhReply', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PH_REPLY',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'PO_NO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }],
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