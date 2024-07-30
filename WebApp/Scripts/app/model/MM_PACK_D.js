Ext.define('WEBAPP.model.MM_PACK_D', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DOCNO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'APPQTY', type: 'string' },
        { name: 'APVQTY', type: 'string' },
        { name: 'APVTIME', type: 'string' },
        { name: 'APVID', type: 'string' },
        { name: 'ACKQTY', type: 'string' },
        { name: 'ACKID', type: 'string' },
        { name: 'ACKTIME', type: 'string' },
        { name: 'STAT', type: 'string' },
        { name: 'RSEQ', type: 'string' },
        { name: 'EXPT_DISTQTY', type: 'string' },
        { name: 'PICK_QTY', type: 'string' },
        { name: 'PICK_USER', type: 'string' },
        { name: 'PICK_TIME', type: 'string' },
        { name: 'APLYITEM_NOTE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' }
    ]
});