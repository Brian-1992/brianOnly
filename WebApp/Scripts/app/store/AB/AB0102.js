Ext.define('WEBAPP.store.AB.AB0102', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MI_CONSUME_DATE',
    pageSize: 1000,
    remoteSort: true,
    sorters: [{ property: 'DATA_DATE', direction: 'ASC' },
        { property: 'DATA_BTIME', direction: 'ASC' },
        { property: 'DATA_ETIME', direction: 'ASC' },
        { property: 'WH_NO', direction: 'ASC' },
        { property: 'MMCODE', direction: 'ASC' },
        { property: 'VISIT_KIND', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0102/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});