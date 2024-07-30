Ext.define('WEBAPP.model.CD0007PICK', {
    extend: 'Ext.data.Model',
    fields: [

        { name: 'ACT_PICK_USERID', type: 'string' },   // 揀貨人員ID
        { name: 'PICK_USERNAME', type: 'string' },     // 揀貨人員NAME
        { name: 'DOCNO_CNT', type: 'string' },         // 總單號數
        { name: 'ITEM_SUM', type: 'string' },          // 總品項數
        { name: 'PICK_QTY_SUM', type: 'string' },      // 總件數
        { name: 'DIFFQTY_SUM', type: 'string' }        // 差異件數
 
    ]
});