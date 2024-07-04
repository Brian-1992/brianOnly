Ext.define('WEBAPP.store.AB.AB0105', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0105',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'SET_YM', direction: 'ASC' },
        { property: 'TOWH', direction: 'ASC' },
        { property: 'MMCODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0105/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});