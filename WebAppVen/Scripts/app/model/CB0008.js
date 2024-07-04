Ext.define('WEBAPP.model.CB0008', {
    extend: 'Ext.data.Model',
    fields: [
        // MI_WHINV
        { name: 'WH_NO', type: 'string' },
        { name: 'INV_QTY', type: 'string' },
        //MI_MAST
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'M_STOREID', type: 'string' },
        { name: 'M_AGENNO', type: 'string' },
        { name: 'M_AGENLAB', type: 'string' },
        // BC_BARCODE
        { name: 'BARCODE', type: 'string' }
    ]
});