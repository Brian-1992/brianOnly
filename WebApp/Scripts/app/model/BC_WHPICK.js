Ext.define('WEBAPP.model.BC_WHPICK', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'PICK_DATE', type: 'date' },
        { name: 'DOCNO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'MMCODDE', type: 'string' },
        { name: 'APPQTY', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'APLYITEM_NOTE', type: 'string' },
        { name: 'MAT_CLASS', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'WEXP_ID', type: 'string' },
        { name: 'STORE_LOC', type: 'string' },
        { name: 'PICK_USERID', type: 'string' },
        { name: 'PICK_SEQ', type: 'string' },
        { name: 'ACT_PICK_USERID', type: 'string' },
        { name: 'ACT_PICK_QTY', type: 'string' },
        { name: 'ACT_PICK_TIME', type: 'date' },
        { name: 'ACT_PICK_NOTE', type: 'string' },
        { name: 'HAS_CONFIRMED', type: 'string' },
        { name: 'BOXNO', type: 'string' },
        { name: 'BARCODE', type: 'string' },
        { name: 'XCATEGORY', type: 'string' },

        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },

        { name: 'LOT_NO', type: 'string' },
        { name: 'WEXP_ID_DESC', type: 'string' }
    ]
});