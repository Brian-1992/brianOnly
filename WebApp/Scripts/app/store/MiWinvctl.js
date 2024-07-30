Ext.define('WEBAPP.store.MiWinvctl', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MiWinvctl',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        timeout: 180000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0048/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});