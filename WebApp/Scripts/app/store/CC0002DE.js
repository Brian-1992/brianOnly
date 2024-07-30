Ext.define('WEBAPP.store.CC0002DE', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.CC0002D',
    pageSize: 1000,
    remoteSort: true,
    sorters: [{ property: 'M_STOREID', direction: 'DESC' }, { property: 'MMCODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CC0002/DetailAllExp',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});