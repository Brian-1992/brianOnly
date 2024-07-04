Ext.define('overrides.selection.CheckboxModel', {
    override: 'Ext.selection.CheckboxModel',

    getHeaderConfig: function () {
        var config = this.callParent();
        if (Ext.isFunction(this.selectable)) {

            config.selectable = this.selectable;
            config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                if (this.selectable(record)) {
                    record.selectable = false;
                    return '';
                }
                record.selectable = true;
                return this.defaultRenderer();
            };
            this.on('beforeselect', function (rowModel, record, index, eOpts) {
                return !this.selectable(record);
            }, this);
        }

        return config;
    },

    updateHeaderState: function () {
        // check to see if all records are selected
        var me = this,
            store = me.store,
            storeCount = store.getCount(),
            views = me.views,
            hdSelectStatus = false,
            selectedCount = 0,
            selected, len, i, notSelectableRowsCount = 0;

        if (!store.isBufferedStore && storeCount > 0) {
            hdSelectStatus = true;
            store.each(function (record) {
                if (!record.selectable) {
                    notSelectableRowsCount++;
                }
            }, this);
            selected = me.selected;

            for (i = 0, len = selected.getCount(); i < len; ++i) {
                if (store.indexOfId(selected.getAt(i).id) > -1) {
                    ++selectedCount;
                }
            }
            hdSelectStatus = storeCount === selectedCount + notSelectableRowsCount;
        }

        if (views && views.length) {
            me.column.setHeaderStatus(hdSelectStatus);
        }
    }
});


Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var WhnoGet = '/api/CB0006/GetWhnoCombo';
    var InidGet = '/api/BC0002/GetInidCombo';
    var StatusGet = '/api/AA0016/GetStatusCombo';
    var GridWhnoGet = '/api/AA0016/GetGridWhnoCombo';
    //var T1Name = "藥局進貨作業";
    var T1Rec = 0;
    var T1LastRec = null;
    //var IsPageLoad = true;
   // var pSize = 100; //分頁時每頁筆數


    var col1_labelWid = 130;
    var col1_Wid = 280;
    var col2_labelWid = 130;
    var col2_Wid = 260;
    var col3_labelWid = 110;
    var col3_Wid = 300;
    var f2_wid = (col1_Wid + col2_Wid + col3_Wid) / 6;
    var f3_wid = (col1_Wid + col2_Wid + col3_Wid) / 4;
    var f4_wid = (col1_Wid + col2_Wid + col3_Wid) / 5;
    var mLabelWidth = 70;
    var mWidth = 180;
    var Dno;
    var kind;

    var ReasonStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "01", "TEXT": "01.班長運送途中破損" },
            { "VALUE": "02", "TEXT": "02.氣送過程破損" },
            { "VALUE": "03", "TEXT": "03.醫護人員取用不慎破損" },
            { "VALUE": "04", "TEXT": "04.未收到藥品" },
            { "VALUE": "05", "TEXT": "05.其他" },
            { "VALUE": "06", "TEXT": "06.破損" },
            { "VALUE": "07", "TEXT": "07.過效期" },
            { "VALUE": "08", "TEXT": "08.變質" }
        ]
    });
    var RStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'], 

        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "B", "TEXT": "B.不補發" },
            { "VALUE": "C", "TEXT": "C.補發不扣庫" },
            { "VALUE": "D", "TEXT": "D.作廢" },
            { "VALUE": "N", "TEXT": "N.未確認" },
            { "VALUE": "Y", "TEXT": "Y.補發扣帳" },

        ]
    });
    var whnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var inidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var STKTRANSKINDStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "1", "TEXT": "一般藥" },
            { "VALUE": "2", "TEXT": "1至3級管制藥" }
        ]
    });
    //var statusStore = Ext.create('Ext.data.Store', {
    //    fields: ['VALUE', 'TEXT'],
    //    data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
    //        { "VALUE": "1301", "TEXT": "申請中" },
    //        { "VALUE": "1302", "TEXT": "補發中" },
    //        { "VALUE": "1399", "TEXT": "已結案" }
    //    ]
    //});
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var gridWhnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var cbInid = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'P0',
        fieldLabel: '申請部門',
        //queryParam: {
        //GRP_CODE: 'UR_INID',
        //DATA_NAME: 'INID_FLAG'
        //},
        //insertEmptyRow: true,
        limit: 20,
        queryUrl: InidGet,
        //forceSelection: true,
    });


    function getWhnoCombo() {
        Ext.Ajax.request({
            url: WhnoGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var whnos = data.etts;
                    if (whnos.length > 0) {
                        whnoStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < whnos.length; i++) {
                            whnoStore.add({ VALUE: whnos[i].VALUE, TEXT: whnos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getStatusCombo() {
        Ext.Ajax.request({
            url: StatusGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var status = data.etts;
                    if (status.length > 0) {
                        statusStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < status.length; i++) {
                            statusStore.add({ VALUE: status[i].VALUE, TEXT: status[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getGridWhnoCombo() {
        Ext.Ajax.request({
            url: GridWhnoGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var gridWhno = data.etts;
                    if (gridWhno.length > 0) {
                        gridWhnoStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < gridWhno.length; i++) {
                            gridWhnoStore.add({ VALUE: gridWhno[i].VALUE, TEXT: gridWhno[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        }
        ,
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '申請日期',
                    name: 'D0',
                    id: 'D0',
                    width: 160
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'D1',
                    id: 'D1',
                    labelWidth: 7,
                  
                    width: 120
                }, {
                    xtype: 'combo',
                    store: whnoStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '申請庫別',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                   
                }, {
                    xtype: 'combo',
                    store: STKTRANSKINDStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '異動類別',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 150
                }, {
                    xtype: 'combo',
                    store: statusStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '狀態',
                    name: 'P4',
                    id: 'P4',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 150
               
               
                }, {
                    xtype: 'label',
                    width:210
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D0',
                id: 'D0',
                width: 160
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'D1',
                id: 'D1',
                labelWidth: 7,

                width: 120
            }, {
                xtype: 'textfield',
                fieldLabel: '申請人員',
                name: 'P3',
                enforceMaxLength: true,
                maxLength: 8
            }, cbInid, {
                xtype: 'combo',
                store: STKTRANSKINDStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                fieldLabel: '異動類別',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                padding: '0 4 0 4',
                width: 150
            }, {
                xtype: 'combo',
                store: statusStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                fieldLabel: '設定狀態',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true,
                padding: '0 4 0 4',
                width: 150
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',

            handler: function () {
                //alert(T1QueryForm.getForm().findField('P1').getValue())
                if (T1QueryForm.getForm().findField('P1').getValue() == 2) {
                    Ext.getCmp('t2gird').getColumns()[9].hide();
                    Ext.getCmp('t2gird').getColumns()[10].hide();
                    Ext.getCmp('t2gird').getColumns()[11].hide();
                    Ext.getCmp('t2gird').getColumns()[12].hide();
                }
                else {
                    Ext.getCmp('t2gird').getColumns()[9].show();
                    Ext.getCmp('t2gird').getColumns()[10].show();
                    Ext.getCmp('t2gird').getColumns()[11].show();
                    Ext.getCmp('t2gird').getColumns()[12].show();
                }
                    T1Load();
               
                

            }
           
         
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('D0').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['DOCNO', 'FLOWID', 'STKTRANSKIND', 'APPDEPT', 'APPTIME', 'USEID', 'USEDEPT', 'FRWH', 'TOWH', 'APP_NAME', 'APPDEPT_NAME']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    pf: 'AA0016',
                    p0: T1QueryForm.getForm().findField('D0').rawValue,
                    p1: T1QueryForm.getForm().findField('D1').rawValue,
                    p3: T1QueryForm.getForm().findField('P1').getValue(),
                    p4: T1QueryForm.getForm().findField('P2').getValue(),
                    p6: T1QueryForm.getForm().findField('P0').getValue(),
                    p7: T1QueryForm.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                T2Store.removeAll();
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0016/QueryME',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['FRWH', 'FRWH_D', 'MMCODE', 'MMNAME_E', 'APVQTY', 'BASE_UNIT', 'GTAPL_REASON', 'APLYITEM_NOTE', 'NRCODE', 'BEDNO', 'SEQ', 'MEDNO', 'CHINNAME', 'ORDERDATE', 'STKTRANSKIND','CONFIRMSWITCH']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T2Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T2Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: Dno
                   
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0016/QueryMEDOCD',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }

    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons:[
        {

            itemId: 'cancelRestart',
            text: '結案',
        

            handler: function () {

               
                var records = T1Grid.getSelectionModel().getSelection();
                console.log(records)
                
                if (records.length == 0) {
                    Ext.Msg.alert('訊息', '請先選擇結案資料!');
           
                    return;
                }
               
                Ext.MessageBox.confirm('訊息', '是否確定結案?', function (btn, text) {
                    if (btn === 'yes')
                        for (var i = 0; i < records.length; i++) {

                            Ext.Ajax.request({
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                url: '/api/AA0016/UpdateEnd',
                                params: {
                                    dno: records[i].data.DOCNO,
                      
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.Msg.alert('訊息', '結案完成!');
                                      
                                        T1Store.load();
                                    }
                                },
                                failure: function (response, options) {

                                }
                            });
                        }
      
                });
            }
        }
        ]
    });

    var T2Tool = Ext.widget('toolbar', {
        items: [
            {
                itemId: 'bt1',
                text: '儲存',
     

                handler: function () {
                    var store1 = T2Grid.getStore();
                    //var store = T2Grid.getSelectionModel().getSelection()[0];
                    var store = T2Grid.getStore().getRange();
                    var recs = store;
                    console.log(recs)

                    if (store.length == 0) {
                        Ext.Msg.alert('訊息', '尚無可儲存的品項資料!');

                        return;
                    }

                    Ext.MessageBox.confirm('提示', '是否確定儲存?', function (btn, text) {
                        if (btn === 'yes') {
                            // 先檢查是否有必填欄位未填
                            for (var i = 0; i < recs.length; i++) {
                                if (recs[i].data['FRWH_D'] == '' || recs[i].data['FRWH_D'] == null
                                    || recs[i].data['APVQTY'] == '' || recs[i].data['APVQTY'] == null
                                    || recs[i].data['CONFIRMSWITCH'] == '' || recs[i].data['CONFIRMSWITCH'] == null
                                    || recs[i].data['GTAPL_RESON'] == '' || recs[i].data['GTAPL_RESON'] == null)
                                {
                                    Ext.Msg.alert('提示', '品項' + recs[i].data['MMCODE'] + '尚有欄位未填!');
                                    return false;
                                }
                            }

                            for (var i = 0; i < recs.length; i++) {
                                Ext.Ajax.request({
                                    url: '/api/AA0016/Update',
                                    method: reqVal_p,
                                    params: recs[i].data,
                                    success: function () {
                                        Ext.Msg.alert('提示', '儲存完成');
                                        T2Load();
                                    },
                                    failure: function () {
                                        alert('falil');
                                    }
                                });

                            }
                        }
                    });




                }
            }],
    });

   

    //Ext.tip.QuickTipManager.init();

    var T1Grid = Ext.create('Ext.grid.Panel', {
        menuDisabled: true,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

       
        //plugins: [
        //    Ext.create("Ext.grid.plugin.CellEditing", {
        //        //clicksToEdit: 1,
        //    })
        //],
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }],
        	
        columns: [
            { xtype: 'rownumberer'},
            { text: '異動單號', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 130 },
            { text: '狀態', dataIndex: 'FLOWID', align: 'left', style: 'text-align:left', menuDisabled: true, width: 70 },
            { text: '申請人員', dataIndex: 'APP_NAME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '申請部門', dataIndex: 'APPDEPT_NAME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '申請日期', dataIndex: 'APPTIME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '申請庫別', dataIndex: 'FRWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '異動類別', dataIndex: 'STKTRANSKIND', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
        
           

        ],
        selModel: Ext.create('Ext.selection.CheckboxModel',{//check box
            selectable: function (record) {
                //console.log(record)
                return record.get('FLOWID') !== '補發中';
            },
        },
        ),
        listeners: {
            select: function (grid, record) {// 選擇gird時顯示該筆製造商資料
                //console.log(record);
                Dno = record.data.DOCNO;
                kind = record.data.STKTRANSKIND
               // alert(kind)
                if (kind == "1至3級管制藥") {
                    //alert(kind)
                    Ext.getCmp('t2gird').columns[6].setVisible(false);
                    Ext.getCmp('t2gird').columns[10].setVisible(false);
                    Ext.getCmp('t2gird').columns[11].setVisible(false);
                    Ext.getCmp('t2gird').columns[12].setVisible(false);
                    Ext.getCmp('t2gird').columns[13].setVisible(false);
                    Ext.getCmp('t2gird').columns[14].setVisible(false);
                    //Ext.getCmp('t2gird').down['BEDNO'].setVisible(false);
                    //Ext.getCmp('t2gird').down['MEDNO'].setVisible(false);
                    //Ext.getCmp('t2gird').down['CHINNAME'].setVisible(false);
                    //Ext.getCmp('t2gird').down['ORDERDATE'].setVisible(false);

                    Ext.getCmp("t2gird").view.refresh();
                }
                else if (kind == "一般藥")
                {
                    Ext.getCmp('t2gird').columns[6].setVisible(true);
                    Ext.getCmp('t2gird').columns[10].setVisible(true);
                    Ext.getCmp('t2gird').columns[11].setVisible(true);
                    Ext.getCmp('t2gird').columns[12].setVisible(true);
                    Ext.getCmp('t2gird').columns[13].setVisible(true);
                    Ext.getCmp('t2gird').columns[14].setVisible(true);
                }
                T2Load();
              
            }
        },
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: 'Dno',
        id: 't2gird',
        menuDisabled: true,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        plugins: [
            Ext.create("Ext.grid.plugin.CellEditing", {
                clicksToEdit: 1,
                listeners: {//用於單筆資料
                    afterrender: function (grid) {//afterrender 等畫面畫完再做
                        if (kind == "1至3級管制藥")
                        {
                            alert(kind)
                            Ext.getCmp('t2gird').down['NRCODE'].setVisible(false);
                            Ext.getCmp('t2gird').down['BEDNO'].setVisible(false);
                            Ext.getCmp('t2gird').down['MEDNO'].setVisible(false);
                            Ext.getCmp('t2gird').down['CHINNAME'].setVisible(false);
                            Ext.getCmp('t2gird').down['ORDERDATE'].setVisible(false);

                            Ext.getCmp("t2gird").view.refresh();
                        }
                      
                    }
                },
            })
        ],
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T2Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: '申請庫別', dataIndex: 'FRWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 130 },
            {
                text: "<span style='color: red'>銷帳庫別</span>",
                dataIndex: 'FRWH_D',
                align: 'left',
                style: 'text-align:left',
                menuDisabled: true,
                css: 'background: #FF0000;',
                width: 80,
                editor: {
                    xtype: 'combo',
                    typeAhead: true,
                    triggerAction: 'all',
                    store: gridWhnoStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: false,
                    listConfig: { width: 180 }
                }
            },

            {
                text: '院內碼', dataIndex: 'MMCODE', align: 'left', style: 'text-align:left', menuDisabled: true,width: 80,
                

            },
            { text: '藥品名稱', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, width: 160 },

            //{ text: 'QID', dataIndex: 'QID',align: 'center', style: 'text-align:center', width: 40, sortable: true },
            {
                text: "<span style='color: red'>補發數量</span>", dataIndex: 'APVQTY', align: 'right', style: 'text-align:left',fontColor:'red', menuDisabled: true, width: 80, editor: {
                    //xtype: 'numberfield',
                    //decimalPrecision: 3,
                    //nValue: 0
                }
            },
            {
                text: "<span style='color: red'>補發類別</span>", dataIndex: 'CONFIRMSWITCH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120,
                editor: {
                    xtype: 'combo',
                    typeAhead: true,
                    triggerAction: 'all',
                    store: RStore,

                    displayField: 'TEXT',
                    valueField: 'TEXT',
                    queryMode: 'local',
                    autoSelect: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,//下拉式選單 editor.....
                }
            },
            { text: '單位', dataIndex: 'BASE_UNIT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 60 },
            {
                text: "<span style='color: red'>補發原因</span>", dataIndex: 'GTAPL_RESON', align: 'left', style: 'text-align:left', menuDisabled: true, width: 160,
                editor: {
                    xtype: 'combo',
                    typeAhead: true,
                    triggerAction: 'all',
                    store: ReasonStore,

                    displayField: 'TEXT',
                    valueField: 'TEXT',
                    queryMode: 'local',
                    autoSelect: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,//下拉式選單 editor.....
                }},
            { text: '備註', dataIndex: 'APLYITEM_NOTE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '病歷號', dataIndex: 'MEDNO', id: 'MEDNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '病房號', dataIndex: 'NRCODE', id: 'NRCODE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '病床號', dataIndex: 'BEDNO',id:'BEDNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '病患姓名', dataIndex: 'CHINNAME', id: 'CHINNAME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '追藥時間', dataIndex: 'ORDERDATE', id: 'ORDERDATE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: 'SEQ', dataIndex: 'SEQ', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },//hidden: true,
            { text: 'DOCNO', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },


           
        ],
        //selModel: Ext.create('Ext.selection.CheckboxModel'),//check box
        
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    function T2Load() {
        T2Store.load({
            params: {
                p0: Dno,
                p1: kind,
            }
        });
    }
    var callableWin = null;
    popWinForm = function (docno) {
        var strUrl = "AB1801?strParam=" + docno;
       // var strUrl = "AA0038_Form?strVtype=" + vType + "&strMmcode=" + strMmcode;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, '藥局領用記錄', viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1QueryForm]
       
        }]
    });
    
    var viewport = Ext.create('Ext.Viewport', {
        id: 'viewport',
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [{
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        //items: [TATabs]
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });
    getWhnoCombo();
    getStatusCombo();
    getGridWhnoCombo();

    T1QueryForm.getForm().findField('D0').setValue(new Date());
    T1QueryForm.getForm().findField('D1').setValue(new Date());
});