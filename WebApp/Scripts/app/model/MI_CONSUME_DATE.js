Ext.define('WEBAPP.model.MI_CONSUME_DATE', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DATA_DATE', type: 'string' },
        { name: 'DATA_BTIME', type: 'string' },
        { name: 'DATA_ETIME', type: 'string' },
        { name: 'WH_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'VISIT_KIND', type: 'string' },
        { name: 'CONSUME_QTY', type: 'string' },
        { name: 'STOCK_UNIT', type: 'string' },
        { name: 'INSU_QTY', type: 'string' },
        { name: 'HOSP_QTY', type: 'string' },
        { name: 'PARENT_ORDERCODE', type: 'string' },
        { name: 'PARENT_CONSUME_QTY', type: 'string' },
        { name: 'CREATEDATETIME', type: 'string' },
        { name: 'PROC_ID', type: 'string' },
        { name: 'PROC_MSG', type: 'string' },
        { name: 'PROC_TYPE', type: 'string' }
    ]
});