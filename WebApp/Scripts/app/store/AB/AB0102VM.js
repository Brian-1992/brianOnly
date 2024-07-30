Ext.define('WEBAPP.store.AB.AB0102VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MI_CONSUME_DATE',
        'WEBAPP.model.HIS_CONSUME_D'
    ],
    stores: {
        Master: {
            model: 'WEBAPP.model.MI_CONSUME_DATE',
            pageSize: 50,
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
                timeout: 900000,
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        Details: {
            model: 'WEBAPP.model.HIS_CONSUME_D',
            pageSize: 50,
            remoteSort: true,
            sorters: [
                { property: 'DATA_DATE', direction: 'ASC' },
            { property: 'DATA_BTIME', direction: 'ASC' },
            { property: 'DATA_ETIME', direction: 'ASC' },
            { property: 'STOCKCODE', direction: 'ASC' },
            { property: 'ORDERCODE', direction: 'ASC' },
            { property: 'VISIT_KIND', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0102/Details',
                timeout: 900000,
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }

});