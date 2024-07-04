Ext.define('WEBAPP.store.AB.AB0091', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ME_DOCD',
    pageSize: 1000,
    remoteSort: true,
    sorters: [{ property: 'POSTID', direction: 'ASC' }, { property: 'DOCNO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0091/All',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});