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
        //check to see if all records are selected
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
    var WhnoGet = '/api/AB0013/GetWhnoCombo';
    var T1Name = "申請補發作業";
    var T1Rec = 0;
    var T1Set = '';
    var T1LastRec, T2LastRec = null;
    var viewModel = Ext.create('WEBAPP.store.AB.AB0012VM');
    var userId, userName, userInid, userInidName, userTaskId;
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
    var viewModel = Ext.create('WEBAPP.store.AB.AB0012VM');

    var whnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var FRWHStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var STKTRANSKINDStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "1", "TEXT": "一般藥" },
            { "VALUE": "2", "TEXT": "1至3級管制藥" }
        ]
    });
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "1301", "TEXT": "申請中" },
            { "VALUE": "1302", "TEXT": "補發中" },
            { "VALUE": "1399", "TEXT": "已結案" }
        ]
    });
    var ReasonStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "01", "TEXT": "01 班長運送途中破損" },
            { "VALUE": "02", "TEXT": "02 氣送過程破損" },
            { "VALUE": "03", "TEXT": "03 醫護人員取用不慎破損" },
            { "VALUE": "04", "TEXT": "04 未收到藥品" },
            { "VALUE": "05", "TEXT": "05 其他" },
            { "VALUE": "06", "TEXT": "06 破損" },
            { "VALUE": "07", "TEXT": "07 過效期" },
            { "VALUE": "08", "TEXT": "08 變質" }
        ]
    });
    var MMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',

        labelWidth: 60,
        width: 200,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0061/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1QueryForm.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {

                //選取下拉項目時，顯示回傳值
                //T1Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                //T1Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
            }
        },
    });
    var UserInfoStore = viewModel.getStore('USER_INFO');
    function UserInfoLoad() {
        UserInfoStore.load(function (records, operation, success) {
            if (success) {
                console.log(records)
                var r = records[0];
                userId = r.get('TUSER');
                userName = r.get('UNA');
                userInid = r.get('INID');
                userInidName = r.get('INID_NAME');
            }
        });
    }

    function getWhnoCombo() {
        Ext.Ajax.request({
            url: '/api/AB0013/GetWhnoCombo',
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
    getWhnoCombo();

    function getFrwhCombo() {
        Ext.Ajax.request({
            url: '/api/AB0013/GetFrwhCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var frwhs = data.etts;
                    if (frwhs.length > 0) {
                        FRWHStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < frwhs.length; i++) {
                            FRWHStore.add({ VALUE: frwhs[i].VALUE, TEXT: frwhs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getFrwhCombo()

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 200;
    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'datefield',
                        fieldLabel: '申請日期',
                        name: 'D0',
                        id: 'D0',
                        padding: '0 4 0 4',
                        width: 160
                    },
                    {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 'D1',
                        id: 'D1',
                        labelWidth: 7,
                        padding: '0 4 0 4',
                        width: 120
                    },
                    {
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
                    },
                    {
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
                        width: 180
                    }, {
                        xtype: 'combo',
                        store: statusStore,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        fieldLabel: '狀態',
                        name: 'P2',
                        id: 'P2',
                        enforceMaxLength: true,
                        padding: '0 4 0 4',
                        width: 160
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            T2Store.removeAll();
                            T1Grid.getSelectionModel().deselectAll();
                            T1Load();
                            msglabel('訊息區:');
                            Ext.getCmp('btnUpdate').setDisabled(true);
                            Ext.getCmp('btnDel').setDisabled(true);
                            Ext.getCmp('btnSubmit').setDisabled(true);
                            Ext.getCmp('btnAdd2').setDisabled(true);
                            Ext.getCmp('btnUpdate2').setDisabled(true);
                            Ext.getCmp('btnDel2').setDisabled(true);
                            // Ext.getCmp('btnCopy').setDisabled(true);
                            // Ext.getCmp('eastform').collapse();
                        }
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
        ]
    });


    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['DOCNO', 'FLOWID', 'STKTRANSKIND', 'APPDEPT', 'APPTIME', 'USEID', 'USEDEPT', 'FRWH', 'TOWH']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

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
                    pf: 'AB0013',
                    p0: T1QueryForm.getForm().findField('D0').rawValue,
                    p1: T1QueryForm.getForm().findField('D1').rawValue,
                    p2: T1QueryForm.getForm().findField('P0').getValue(),
                    p3: T1QueryForm.getForm().findField('P1').getValue(),
                    p4: T1QueryForm.getForm().findField('P2').getValue(),

                };
                Ext.apply(store.proxy.extraParams, np);
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
        fields: ['FRWH', 'FRWH_D', 'MMCODE', 'MMNAME_E', 'APVQTY', 'APPQTY', 'BASE_UNIT', 'GTAPL_RESON', 'APLYITEM_NOTE', 'NRCODE', 'BEDNO', 'SEQ', 'MEDNO', 'CHINNAME', 'ORDERDATE', 'STKTRANSKIND', 'DOCNO']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

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
                    p0: Dno,
                    p1: kind

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0013/QueryMEDOCD',
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
        buttons: [
            //{
            //    text: '查詢',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //        T1Form.setVisible(false);
            //        T1Query.setVisible(true);
            //    }
            //},
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    //var r = WEBAPP.model.ME_DOCM.create({
                    //    //APPNAME: 'aaa'
                    //});
                    //T1Store.add(r.copy());
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AB0013/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AB0013/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    if (item.get('FLOWID') == '申請中') {
                        //        name += '「' + item.get('DOCNO') + '」<br>';
                        //        docno += item.get('DOCNO') + ',';
                        //    }
                        //});
                        $.map(selection, function (item, key) {
                            if (item.get('FLOWID') == '申請中') {
                                name += '「' + item.get('DOCNO') + '」<br>';
                                docno += item.get('DOCNO') + ',';
                            }
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0013/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            T1Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {

                text: '提出申請',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    if (item.get('FLOWID') == '申請中') {
                        //        name += '「' + item.get('DOCNO') + '」<br>';
                        //        docno += item.get('DOCNO');
                        //    }
                        //});
                        $.map(selection, function (item, key) {
                            if (item.get('FLOWID') == '申請中') {
                                name += '「' + item.get('DOCNO') + '」<br>';
                                docno += item.get('DOCNO');
                            }
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否提出申請?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0013/UpdateEnd',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:送出成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1QueryForm]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                autoScroll: true,
                items: [T1Tool]
            }],

        selType: 'checkboxmodel',
        columns: [
            { xtype: 'rownumberer' },
            { text: '異動單號', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 140 },
            { text: '狀態', dataIndex: 'FLOWID', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '申請日期', dataIndex: 'APPTIME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '申請庫別', dataIndex: 'FRWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '異動類別', dataIndex: 'STKTRANSKIND', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '藥品名稱', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 160 },
            //{ text: '藥品名稱', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 160 },


        ],
        //selModel: Ext.create('Ext.selection.CheckboxModel', {//check box
        //    selectable: function (record) {
        //        console.log(record)
        //        return record.get('FLOWID') !== '申請中';
        //    },
        //}),
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T1LastRec != null) {
                        if (T1LastRec.data["FLOWID"] !== '申請中') {
                            Ext.getCmp('btnUpdate').setDisabled(true);
                            Ext.getCmp('btnDel').setDisabled(true);
                            Ext.getCmp('btnSubmit').setDisabled(true);

                            Ext.getCmp('btnUpdate2').setDisabled(true);
                            Ext.getCmp('btnDel2').setDisabled(true);
                            Ext.getCmp('btnAdd2').setDisabled(true);
                        }
                        else {
                            Ext.getCmp('btnUpdate').setDisabled(false);
                            Ext.getCmp('btnDel').setDisabled(false);
                            Ext.getCmp('btnSubmit').setDisabled(false);

                            Ext.getCmp('btnUpdate2').setDisabled(true);
                            Ext.getCmp('btnDel2').setDisabled(true);
                            
                            Ext.getCmp('btnAdd2').setDisabled(false);
                        }



                        //Ext.getCmp('btnCopy').setDisabled(false);
                        var records = T1Grid.getSelectionModel().getSelection();
                       
                        //
                        //T1QueryForm.getForm().findField('P0').setValue(records[0].data.FRWH);
                        //alert(Dno)
                        if (T1Set === '')
                            setFormT1a();
                        T2Load();
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1Rec > 0)
                {
                    Dno = records[0].data.DOCNO;
                    kind = records[0].data.STKTRANSKIND;
                    //console.log(records)
                    setFormT1a();
                    T2Load();
                    if (kind == "一般藥") {
                        //Ext.getCmp('t2grid').columns[8].setVisible(true);
                        Ext.getCmp('t2grid').columns[9].setVisible(true);
                        Ext.getCmp('t2grid').columns[10].setVisible(true);
                        Ext.getCmp('t2grid').columns[11].setVisible(true);
                        Ext.getCmp('t2grid').columns[12].setVisible(true);
                        Ext.getCmp('t2grid').columns[13].setVisible(true);
                    }
                    else {
                        Ext.getCmp('t2grid').columns[9].setVisible(false);
                        Ext.getCmp('t2grid').columns[10].setVisible(false);
                        Ext.getCmp('t2grid').columns[11].setVisible(false);
                        Ext.getCmp('t2grid').columns[12].setVisible(false);
                        Ext.getCmp('t2grid').columns[13].setVisible(true);
                    }
                }
                
               // t2grid.view.refresh();
                if (T1LastRec != null) {
                    if (T1LastRec.data["FLOWID"] !== '申請中') {
                        Ext.getCmp('btnUpdate').setDisabled(true);
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnSubmit').setDisabled(true);

                        Ext.getCmp('btnUpdate2').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(true);
                        Ext.getCmp('btnAdd2').setDisabled(true);
                    }
                    else {
                        Ext.getCmp('btnUpdate').setDisabled(false);
                        Ext.getCmp('btnDel').setDisabled(false);
                        Ext.getCmp('btnSubmit').setDisabled(false);

                        Ext.getCmp('btnUpdate2').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(true);

                        Ext.getCmp('btnAdd2').setDisabled(false);
                    }
                }
               
            },
            sortchange: function (ct, col, dir, eOpts) {
                Ext.getCmp('btnUpdate').setDisabled(true);
                Ext.getCmp('btnDel').setDisabled(true);
                Ext.getCmp('btnSubmit').setDisabled(true);
            }
        }
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //        T1Form.setVisible(false);
            //        T1Query.setVisible(true);
            //    }
            //},
            {
                text: '新增',
                id: 'btnAdd2',
                name: 'btnAdd2',
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AB0013/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AB0013/UpdateD';
                    setFormT2('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel2',
                name: 'btnDel2',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        //let name = '';
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    //name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO');
                        //    seq += item.get('SEQ');
                        //});
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO');
                            seq += item.get('SEQ');
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0013/DeleteD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            T2Grid.getSelectionModel().deselectAll();
                                            Ext.getCmp('btnUpdate2').setDisabled(true);
                                            Ext.getCmp('btnDel2').setDisabled(true);
                                            T2Load();
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
    });



    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        id:'t2grid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T2Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: '申請庫別', dataIndex: 'FRWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '銷帳庫別', dataIndex: 'FRWH_D', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80, },

            {
                text: '院內碼', dataIndex: 'MMCODE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80,


            },
            { text: '藥品名稱', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, width: 160 },

            //{ text: 'QID', dataIndex: 'QID',align: 'center', style: 'text-align:center', width: 40, sortable: true },
            {
                text: '補發數量', dataIndex: 'APPQTY', align: 'right', style: 'text-align:left', fontColor: 'red', menuDisabled: true, width: 80,
            },
            { text: '單位', dataIndex: 'BASE_UNIT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 60 },
            { text: '補發原因', dataIndex: 'GTAPL_RESON', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '備註', dataIndex: 'APLYITEM_NOTE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '病歷號', dataIndex: 'MEDNO', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },
            { text: '病房號', dataIndex: 'NRCODE', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },
            { text: '病床號', dataIndex: 'BEDNO', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },
            { text: '病患姓名', dataIndex: 'CHINNAME', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },
            { text: '追藥時間', dataIndex: 'ORDERDATE', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },
            { text: 'DOCNO', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },
            { text: 'SEQ', dataIndex: 'SEQ', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },
            //selModel: Ext.create('Ext.selection.CheckboxModel'),//check box
            {
                header: "",
                flex: 1
            }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null) {
                        if (T1LastRec.data["FLOWID"] !== '申請中') {
                            Ext.getCmp('btnUpdate2').setDisabled(true);
                            Ext.getCmp('btnDel2').setDisabled(true);
                            Ext.getCmp('btnAdd2').setDisabled(true);
                        }
                        else {
                            Ext.getCmp('btnUpdate2').setDisabled(false);
                            Ext.getCmp('btnDel2').setDisabled(false);
                            Ext.getCmp('btnAdd2').setDisabled(false);
                        }
                        // Ext.getCmp('btnCopy').setDisabled(true);

                        if (T1Set === '')
                            setFormT2a();
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            },
            sortchange: function (ct, col, dir, eOpts) {
                Ext.getCmp('btnUpdate2').setDisabled(true);
                Ext.getCmp('btnDel2').setDisabled(true);
            }
        }
    });


    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model'); // /Scripts/app/model/MiUnitexch.js
            //T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            //u = f.findField("TOWH"); // 廠商碼在新增時才可填寫
            //u.setReadOnly(false);
            //u.clearInvalid();
            //f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A

            var date = new Date();

            f.findField("FRWH").setReadOnly(false);
            f.findField("STKTRANSKIND").setReadOnly(false);
            f.findField("FRWH").setValue('');
            f.findField("DOCNO").setValue('系統自編');
            f.findField("APPID").setValue(userId + ' ' + userName);
            f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);
            f.findField("APPTIME").setValue(getChtToday());
            f.findField("FRWH").setValue('');
            f.findField("STKTRANSKIND").setValue('');
        }
        else {
            // u = f.findField('TOWH');

            f.findField("FRWH").setReadOnly(false);
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            //var tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
            //f.findField("TOWH").setValue(tmpArray[0]);

            // 因為使用部門和領用庫房有連動, 所以都要重新取得
            //Ext.Ajax.request({
            //    url: '/api/AB0010/GetGrade',
            //    method: reqVal_p,
            //    params: {
            //        WH_NO: tmpArray[0]
            //    },
            //    success: function (response) {
            //        var data = Ext.decode(response.responseText);
            //        T4Load(data);

            //        tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
            //        f.findField('TOWH').setValue(tmpArray[0]);
            //    },
            //    failure: function (response) {
            //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            //    },
            //});

        }
        f.findField('x').setValue(x);
        //f.findField('TOWH').setReadOnly(false);
        //f.findField('FRWH').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        // u.focus();
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("FLOWID").setValue(T1LastRec.data["FLOWID"]);
            //f.findField("INID_NAME").setValue(T1LastRec.data["FRWH"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH"]);
            f.findField("STKTRANSKIND").setValue(T1LastRec.data["STKTRANSKIND"]);
            //f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            //var tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
            //f.findField('TOWH').setValue(tmpArray[0]);

            //// 因為使用部門和領用庫房有連動, 所以都要重新取得
            //Ext.Ajax.request({
            //    url: '/api/AB0010/GetGrade',
            //    method: reqVal_p,
            //    params: {
            //        WH_NO: tmpArray[0]
            //    },
            //    success: function (response) {
            //        var data = Ext.decode(response.responseText);
            //        T4Load(data);

            //        tmpArray = T1LastRec.data["FRWH_NAME"].split(" ");

            //        // 切換T1Grid時, 如果值一樣,還做setValue()的話,欄位會被清空,所以值不同再做setValue()
            //        if (f.findField('FRWH').getValue() != tmpArray[0])
            //            f.findField('FRWH').setValue(tmpArray[0]);
            //    },
            //    failure: function (response) {
            //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            //    },
            //});

            //f.findField('TOWH').setReadOnly(true);
            //f.findField('FRWH').setReadOnly(true);
            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
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
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '申請日期',
                name: 'APPTIME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'APPID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '狀態',
                name: 'FLOWID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true

            }, {
                xtype: 'displayfield',
                fieldLabel: '申請單位',
                name: 'INID_NAME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'combo',
                store: FRWHStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                fieldLabel: '申請庫別',
                name: 'FRWH',
                readOnly: true,
                enforceMaxLength: true,
                padding: '0 4 0 4',
                submitValue: true,
                width: 150,
                fieldCls: 'required',
                allowBlank: false
            },, {
                xtype: 'combo',
                store: STKTRANSKINDStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                fieldLabel: '異動類別',
                name: 'STKTRANSKIND',
                readOnly: true,
                enforceMaxLength: true,
                padding: '0 4 0 4',
                submitValue: true,
                width: 150,
                fieldCls: 'required',
                allowBlank: false
            }

        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                        //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                        //else {
                        //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        //        if (btn === 'yes') {
                        //            T1Submit();
                        //        }
                        //    });
                        //}

                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                                //T1Set = '';
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });


    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1QueryForm.getForm().reset();
                            T1QueryForm.getForm().findField('D0').setValue(new Date());
                            T1QueryForm.getForm().findField('D1').setValue(new Date());
                            var v = action.result.etts[0];
                            T2Store.removeAll();
                            // T1QueryForm.getForm().findField('P1').setValue(v.DOCNO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            T1Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                    }

                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    function T1Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
    }

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();

        var f = T2Form.getForm();
        //alert(kind)
        
        if (x === "I") {
            isNew = true;
            f.reset();
            var r = Ext.create('T2Model'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            //u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            //u.setReadOnly(false);
            //u.clearInvalid();

            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField('MMCODE').setReadOnly(false);
            // f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
        }
        else {
            u = f.findField('MMCODE');

            f.findField("DOCNO").setValue(T2LastRec.data["DOCNO"]);
            //f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('GTAPL_RESON').setValue(T2LastRec.data["GTAPL_RESON"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);
            f.findField('SEQ').setValue(T2LastRec.data["SEQ"]);
        }
        f.findField('x').setValue(x);
        //f.findField('MMCODE').setReadOnly(false);
        f.findField('APPQTY').setReadOnly(false);
        f.findField('BASE_UNIT').setReadOnly(false);
        f.findField('GTAPL_RESON').setReadOnly(false);
        f.findField('APLYITEM_NOTE').setReadOnly(false);
        if (kind == "一般藥") {
            //Ext.getCmp("BEDNO").setVisible(true);
            //Ext.getCmp("MEDNO").setVisible(true);
            //Ext.getCmp("CHINNAME").setVisible(true);
            //Ext.getCmp("ORDERDATE").setVisible(true); 20191030
            f.findField('MEDNO').setReadOnly(false);
            f.findField('NRCODE').setVisible(true);
            f.findField('BEDNO').setVisible(true);
            f.findField('MEDNO').setVisible(true);
            f.findField('CHINNAME').setVisible(true);
            f.findField('ORDERDATE').setVisible(true);
        }
        else {
            //Ext.getCmp("BEDNO").setVisible(false);
            //Ext.getCmp("MEDNO").setVisible(false);
            //Ext.getCmp("CHINNAME").setVisible(false);
            //Ext.getCmp("ORDERDATE").setVisible(false); 20191030
            f.findField('NRCODE').setVisible(false);
            f.findField('BEDNO').setVisible(false);
            f.findField('MEDNO').setVisible(false);
            f.findField('CHINNAME').setVisible(false);
            f.findField('ORDERDATE').setVisible(false);
        }
        // T2Form.down('#btnMmcode').setVisible(true);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        //u.focus();
    }

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        var f = T2Form.getForm();
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            f.findField("DOCNO").setValue(T2LastRec.data["DOCNO"]);
            //f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            //f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            /// f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('GTAPL_RESON').setValue(T2LastRec.data["GTAPL_RESON"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);
            f.findField('SEQ').setValue(T2LastRec.data["SEQ"]);
            f.findField('FRWH_D').setValue(T2LastRec.data["FRWH_D"]);
            f.findField('NRCODE').setValue(T2LastRec.data["NRCODE"]);
            f.findField('BEDNO').setValue(T2LastRec.data["BEDNO"]);
            f.findField('MEDNO').setValue(T2LastRec.data["MEDNO"]);
            f.findField('CHINNAME').setValue(T2LastRec.data["CHINNAME"]);
            f.findField('ORDERDATE').setValue(T2LastRec.data["ORDERDATE"]);
            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
            f.findField('MEDNO').setReadOnly(true);
            // T2Form.down('#btnMmcode').setVisible(false);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);
        }
        if (kind == "一般藥") {
            //Ext.getCmp("BEDNO").setVisible(true);
            //Ext.getCmp("MEDNO").setVisible(true);
            //Ext.getCmp("CHINNAME").setVisible(true);
            //Ext.getCmp("ORDERDATE").setVisible(true); 20191030
            f.findField('NRCODE').setVisible(true);
            f.findField('BEDNO').setVisible(true);
            f.findField('MEDNO').setVisible(true);
            f.findField('CHINNAME').setVisible(true);
            f.findField('ORDERDATE').setVisible(true);
        }
        else {
            //Ext.getCmp("BEDNO").setVisible(false);
            //Ext.getCmp("MEDNO").setVisible(false);
            //Ext.getCmp("CHINNAME").setVisible(false);
            //Ext.getCmp("ORDERDATE").setVisible(false); 20191030
            f.findField('NRCODE').setVisible(false);
            f.findField('BEDNO').setVisible(false);
            f.findField('MEDNO').setVisible(false);
            f.findField('CHINNAME').setVisible(false);
            f.findField('ORDERDATE').setVisible(false);
        }
    }

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            // f.findField("MMNAME_C").setValue(args.MMNAME_C);
            // f.findField("MMNAME_E").setValue(args.MMNAME_E);
            f.findField("BASE_UNIT").setValue(args.BASE_UNIT);
        }
    }
    function T1Load() {
        T1Tool.moveFirst();
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    function T2Load() {
        T2Store.load({
            params: {
                p0: Dno,
                p1: kind,
            }
        });
    }
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        id: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,
        labelAlign: 'right',
        msgTarget: 'side',
        labelWidth: 60,
        width: 300,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0013/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T2Form.getForm().findField('MMCODE').getValue(),
                p2:kind//P0:預設是動態MMCODE
               // T1QueryForm.getForm().findField('P0').getValue(),
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    //f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));   
                }
            }
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'vbox',
        id:'T2Form',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 60,
            width: 300,
        },
        defaultType: 'textfield',
        items: [{
            name: 'x',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請單號',
            readOnly: true,
            name: 'DOCNO',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '銷帳庫別',
            readOnly: true,
            name: 'FRWH_D'
        },
            mmCodeCombo,
        {
            xtype: 'displayfield',
            fieldLabel: "藥品名稱",
            name: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 300,
            readOnly: true
        }, {
            fieldLabel: "補發數量",
            name: 'APPQTY',
            style: 'text-align:left',
            align: 'left',
            allowBlank: false,
            fieldCls: 'required',
            width: 300
        }, {
            fieldLabel: "單位",
            name: 'BASE_UNIT',
            style: 'text-align:left',
            align: 'left',
            readOnly: true,
            width: 300
        }, {
            xtype: 'combo',
            fieldLabel: "補發原因",
            name: 'GTAPL_RESON',
            style: 'text-align:left',
            align: 'left',
            readOnly: true,
            store: ReasonStore,
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            padding: '0 4 0 4',
            width: 300

        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APLYITEM_NOTE',
            enforceMaxLength: true,
            maxLength: 400,
            height: 200,
            readOnly: true
        }, {
            fieldLabel: "病歷號",
            name: 'MEDNO',
            style: 'text-align:left',
            align: 'left',
            id: 'MEDNO',
            readOnly: true,
            hidden: true

        }, {
            xtype: 'displayfield',
            fieldLabel: "病房號",
            name: 'NRCODE',
            id: 'NRCODE',
            style: 'text-align:left',
            align: 'left',
            readOnly: true,
            hidden: true
        }, {
            xtype: 'displayfield',
            fieldLabel: "病床號",
            name: 'BEDNO',
            id: 'BEDNO',
            style: 'text-align:left',
            align: 'left',
            readOnly: true,
            //id: 'BEDNO',
            hidden: true      
        }, {
            xtype: 'displayfield',
            fieldLabel: "病患姓名",
            name: 'CHINNAME',
            style: 'text-align:left',
            align: 'left',
            readOnly: true,
            id: 'CHINNAME',
            hidden: true
        }, {
            xtype: 'displayfield',
            fieldLabel: "追藥時間",
            name: 'ORDERDATE',
            style: 'text-align:left',
            align: 'left',
            readOnly: true,
            id: 'ORDERDATE',
            hidden: true
        }, {
            fieldLabel: "SEQ",
            name: 'SEQ',
            style: 'text-align:left',
            align: 'left',
            hidden: true

        }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                        //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                        //else {
                        //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        //        if (btn === 'yes') {
                        //            T1Submit();
                        //        }
                        //    });
                        //}

                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                                //T1Set = '';
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T2Cleanup
            }
        ]
    });

    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.DN);
                            T2Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break; 
                        case "U":
                            T2Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                    }

                    T2Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    function T2Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '瀏覽',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                //items: [T1Query, T1Form]
                items: [T1Form, T2Form]
            }
        ]
    });
    UserInfoLoad();
    //T3Load();
    Ext.getCmp('eastform').collapse();

    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    //Ext.getCmp('btnCopy').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);

    T1QueryForm.getForm().findField('D0').setValue(new Date());
    T1QueryForm.getForm().findField('D1').setValue(new Date());
});









//Ext.define('overrides.selection.CheckboxModel', {
//    override: 'Ext.selection.CheckboxModel',

//    getHeaderConfig: function () {
//        var config = this.callParent();
//        if (Ext.isFunction(this.selectable)) {

//            config.selectable = this.selectable;
//            config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
//                if (this.selectable(record)) {
//                    record.selectable = false;
//                    return '';
//                }
//                record.selectable = true;
//                return this.defaultRenderer();
//            };
//            this.on('beforeselect', function (rowModel, record, index, eOpts) {
//                return !this.selectable(record);
//            }, this);
//        }

//        return config;
//    },

//    updateHeaderState: function () {
//         check to see if all records are selected
//        var me = this,
//            store = me.store,
//            storeCount = store.getCount(),
//            views = me.views,
//            hdSelectStatus = false,
//            selectedCount = 0,
//            selected, len, i, notSelectableRowsCount = 0;

//        if (!store.isBufferedStore && storeCount > 0) {
//            hdSelectStatus = true;
//            store.each(function (record) {
//                if (!record.selectable) {
//                    notSelectableRowsCount++;
//                }
//            }, this);
//            selected = me.selected;

//            for (i = 0, len = selected.getCount(); i < len; ++i) {
//                if (store.indexOfId(selected.getAt(i).id) > -1) {
//                    ++selectedCount;
//                }
//            }
//            hdSelectStatus = storeCount === selectedCount + notSelectableRowsCount;
//        }

//        if (views && views.length) {
//            me.column.setHeaderStatus(hdSelectStatus);
//        }
//    }
//});


//Ext.Loader.setConfig({
//    enabled: true,
//    paths: {
//        'WEBAPP': '/Scripts/app'
//    }
//});

//Ext.require(['WEBAPP.utils.Common']);

//Ext.onReady(function () {
//    var WhnoGet = '/api/CB0006/GetWhnoCombo';
//    var T1Name = "申請補發作業";
//    var T1Rec = 0;
//    var T1Set = '';
//    var T1LastRec = null;
//    var viewModel = Ext.create('WEBAPP.store.AB.AB0012VM');
//    var userId, userName, userInid, userInidName, userTaskId;
//    var IsPageLoad = true;
//     var pSize = 100; //分頁時每頁筆數


//    var col1_labelWid = 130;
//    var col1_Wid = 280;
//    var col2_labelWid = 130;
//    var col2_Wid = 260;
//    var col3_labelWid = 110;
//    var col3_Wid = 300;
//    var f2_wid = (col1_Wid + col2_Wid + col3_Wid) / 6;
//    var f3_wid = (col1_Wid + col2_Wid + col3_Wid) / 4;
//    var f4_wid = (col1_Wid + col2_Wid + col3_Wid) / 5;
//    var mLabelWidth = 70;
//    var mWidth = 180;
//    var Dno;
//    var kind;
//    var viewModel = Ext.create('WEBAPP.store.AB.AB0012VM');

//    var whnoStore = Ext.create('Ext.data.Store', {
//        fields: ['VALUE', 'TEXT']
//    });
//    var STKTRANSKINDStore = Ext.create('Ext.data.Store', {
//        fields: ['VALUE', 'TEXT'],
//        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
//            { "VALUE": "1", "TEXT": "一般藥" },
//            { "VALUE": "2", "TEXT": "1至3級管制藥" }
//        ]
//    });
//    var statusStore = Ext.create('Ext.data.Store', {
//        fields: ['VALUE', 'TEXT'],
//        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
//            { "VALUE": "1301", "TEXT": "申請中" },
//            { "VALUE": "1302", "TEXT": "補發中" },
//            { "VALUE": "1399", "TEXT": "已結案" }
//        ]
//    });
//    var ReasonStore = Ext.create('Ext.data.Store', {
//        fields: ['VALUE', 'TEXT'],
//        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
//            { "VALUE": "01", "TEXT": "01 班長運送途中破損" },
//            { "VALUE": "02", "TEXT": "02 氣送過程破損" },
//            { "VALUE": "03", "TEXT": "03 醫護人員取用不慎破損" },
//            { "VALUE": "04", "TEXT": "04 未收到藥品" },
//            { "VALUE": "05", "TEXT": "05 其他" },
//            { "VALUE": "06", "TEXT": "06.破損" },
//            { "VALUE": "07", "TEXT": "07.過效期" },
//            { "VALUE": "08", "TEXT": "08.變質" }
//        ]
//    });
//    var MMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
//        id: 'MMCODE',
//        name: 'MMCODE',
//        fieldLabel: '院內碼',
//        labelAlign: 'right',
//        labelWidth: 60,
//        width: 200,
//        limit: 10, //限制一次最多顯示10筆
//        queryUrl: '/api/AA0061/GetMMCODECombo', //指定查詢的Controller路徑
//        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
//        getDefaultParams: function () { //查詢時Controller固定會收到的參數
//            return {
//                p1: T1QueryForm.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
//            };
//        },
//        listeners: {
//            select: function (c, r, i, e) {

//                選取下拉項目時，顯示回傳值
//                T1Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
//                T1Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
//            }
//        },
//    });
//    var UserInfoStore = viewModel.getStore('USER_INFO');
//    function UserInfoLoad() {
//        UserInfoStore.load(function (records, operation, success) {
//            if (success) {
//                console.log(records)
//                var r = records[0];
//                userId = r.get('TUSER');
//                userName = r.get('UNA');
//                userInid = r.get('INID');
//                userInidName = r.get('INID_NAME');
//            }
//        });
//    }

//    function getWhnoCombo() {
//        Ext.Ajax.request({
//            url: WhnoGet,
//            method: reqVal_g,
//            success: function (response) {
//                var data = Ext.decode(response.responseText);
//                if (data.success) {
//                    var whnos = data.etts;
//                    if (whnos.length > 0) {
//                        whnoStore.add({ VALUE: '', TEXT: '' });
//                        for (var i = 0; i < whnos.length; i++) {
//                            whnoStore.add({ VALUE: whnos[i].VALUE, TEXT: whnos[i].TEXT });
//                        }
//                    }
//                }
//            },
//            failure: function (response, options) {

//            }
//        });
//    }



//    var T1QueryForm = Ext.widget({
//        xtype: 'form',
//        layout: 'form',
//        frame: false,
//        cls: 'T1b',
//        title: '',
//        autoScroll: true,
//        bodyPadding: '5 5 0',
//        fieldDefaults: {
//            labelAlign: 'right',
//            msgTarget: 'side',
//            labelWidth: 90
//        },
//        defaultType: 'textfield',
//        items: [
//            {
//                xtype: 'datefield',
//                fieldLabel: '申請日期',
//                name: 'D0',
//                id: 'D0',
//                width: 160
//            }, {
//                xtype: 'datefield',
//                fieldLabel: '至',
//                name: 'D1',
//                id: 'D1',
//                labelWidth: 7,

//                width: 120
//            }, {
//                xtype: 'combo',
//                store: whnoStore,
//                displayField: 'TEXT',
//                valueField: 'VALUE',
//                queryMode: 'local',
//                fieldLabel: '申請庫別',
//                name: 'P0',
//                id: 'P0',
//                enforceMaxLength: true,
//                padding: '0 4 0 4',

//            }, {
//                xtype: 'combo',
//                store: STKTRANSKINDStore,
//                displayField: 'TEXT',
//                valueField: 'VALUE',
//                queryMode: 'local',
//                fieldLabel: '異動類別',
//                name: 'P1',
//                id: 'P1',
//                enforceMaxLength: true,
//                padding: '0 4 0 4',
//                width: 150
//            }, {
//                xtype: 'combo',
//                store: statusStore,
//                displayField: 'TEXT',
//                valueField: 'VALUE',
//                queryMode: 'local',
//                fieldLabel: '狀態',
//                name: 'P2',
//                id: 'P2',
//                enforceMaxLength: true,
//                padding: '0 4 0 4',
//                width: 150
//            }
//        ],
//        buttons: [{
//            itemId: 'query', text: '查詢',
//            handler: T1Load
//        }, {
//            itemId: 'clean', text: '清除', handler: function () {
//                var f = this.up('form').getForm();
//                f.reset();
//                f.findField('D0').focus(); // 進入畫面時輸入游標預設在P0
//            }
//        }]
//    });

//    Ext.define('T1Model', {
//        extend: 'Ext.data.Model',
//        fields: ['DOCNO', 'FLOWID', 'STKTRANSKIND', 'APPDEPT', 'APPTIME', 'USEID', 'USEDEPT', 'FRWH', 'TOWH']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

//    });

//    var T1Store = Ext.create('Ext.data.Store', {
//         autoLoad:true,
//        model: 'T1Model',
//        pageSize: 20,
//        remoteSort: true,
//        sorters: [{ property: 'DOCNO', direction: 'ASC' }],

//        listeners: {
//            beforeload: function (store, options) {
//                var np = {
//                    p0: T1QueryForm.getForm().findField('D0').rawValue,
//                    p1: T1QueryForm.getForm().findField('D1').rawValue,
//                    p2: T1QueryForm.getForm().findField('P0').getValue(),
//                    p3: T1QueryForm.getForm().findField('P1').getValue(),
//                    p4: T1QueryForm.getForm().findField('P2').getValue(),

//                };
//                Ext.apply(store.proxy.extraParams, np);
//            }
//        },

//        proxy: {
//            type: 'ajax',
//            actionMethods: {
//                read: 'POST' // by default GET
//            },
//            url: '/api/AA0016/QueryME',
//            reader: {
//                type: 'json',
//                rootProperty: 'etts',
//                totalProperty: 'rc'
//            }
//        }

//    });

//    Ext.define('T2Model', {
//        extend: 'Ext.data.Model',
//        fields: ['FRWH', 'FRWH_D', 'MMCODE', 'MMNAME_E', 'APVQTY', 'APPQTY', 'BASE_UNIT', 'GTAPL_RESON', 'APLYITEM_NOTE', 'BEDNO', 'SEQ', 'MEDNO', 'CHINNAME', 'ORDERDATE', 'STKTRANSKIND','DOCNO']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

//    });

//    var T2Store = Ext.create('Ext.data.Store', {
//         autoLoad:true,
//        model: 'T2Model',
//        pageSize: 20,
//        remoteSort: true,
//        sorters: [{ property: 'MMCODE', direction: 'ASC' }],

//        listeners: {
//            beforeload: function (store, options) {
//                var np = {
//                    p0: Dno,
//                    p1: kind

//                };
//                Ext.apply(store.proxy.extraParams, np);
//            }
//        },

//        proxy: {
//            type: 'ajax',
//            actionMethods: {
//                read: 'POST' // by default GET
//            },
//            url: '/api/AB0013/QueryMEDOCD',
//            reader: {
//                type: 'json',
//                rootProperty: 'etts'
//            }
//        }

//    });

//    var T1Tool = Ext.create('Ext.PagingToolbar', {
//        store: T1Store,
//        displayInfo: true,
//        border: false,
//        plain: true,
//        buttons: [
//            {
//                text: '新增',
//                id: 'btnAdd',
//                name: 'btnAdd',
//                handler: function () {
//                    T1Form1.setVisible(true);
//                    T1Form.setVisible(false);
//                    T1Set = '/api/AB0013/MasterCreate';
//                    setFormT1F('I', '新增');
//                }
//            },
//            {
//                text: '修改',
//                id: 'btnUpdate',
//                name: 'btnUpdate',
//                handler: function () {
//                    T1Form1.setVisible(true);
//                    T1Form.setVisible(false);
//                    T1Set = '/api/AB0013/MasterUpdate';
//                    setFormT1F('U', '修改');
//                }
//            },
//            {
//                text: '刪除',
//                id: 'btnDel',
//                name: 'btnDel',
//                handler: function () {
//                    var selection = T1Grid.getSelection();
//                    if (selection.length) {
//                        let name = '';
//                        let docno = '';
//                        selection.map(item => {
//                            name += '「' + item.get('DOCNO') + '」<br>';
//                            docno += item.get('DOCNO') + ',';
//                        });

//                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
//                            if (btn === 'yes') {
//                                Ext.Ajax.request({
//                                    url: '/api/AB0013/MasterDelete',
//                                    method: reqVal_p,
//                                    params: {
//                                        DOCNO: docno
//                                    },
//                                    success: function (response) {
//                                        var data = Ext.decode(response.responseText);
//                                        if (data.success) {
//                                            msglabel('訊息區:刪除成功');
//                                            T2Store.removeAll();
//                                            T1Grid.getSelectionModel().deselectAll();
//                                            T1Load();
//                                            Ext.getCmp('btnSubmit').setDisabled(true);
//                                        }
//                                        else {
//                                            Ext.MessageBox.alert('錯誤', data.msg);
//                                            msglabel('訊息區:' + data.msg);
//                                        }
//                                    },
//                                    failure: function (response) {
//                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
//                                    }
//                                });
//                            }
//                        });
//                    }
//                }
//            }, {
//            text: '提出申請',
//            id: 'btnSubmit',
//            name: 'btnSubmit',


//                handler: function () {


//                    var records = T1Grid.getSelectionModel().getSelection();
//                    console.log(records)

//                    if (records.length == 0) {
//                        Ext.Msg.alert('訊息', '請先選擇申請資料!');

//                        return;
//                    }

//                    Ext.MessageBox.confirm('訊息', '是否確定申請?', function (btn, text) {
//                        if (btn === 'yes')
//                            for (var i = 0; i < records.length; i++) {

//                                Ext.Ajax.request({
//                                    actionMethods: {
//                                        read: 'POST' // by default GET
//                                    },
//                                    url: '/api/AB0013/UpdateEnd',
//                                    params: {
//                                        dno: records[i].data.DOCNO,

//                                    },
//                                    success: function (response) {
//                                        var data = Ext.decode(response.responseText);
//                                        if (data.success) {
//                                            Ext.Msg.alert('訊息', '申請完成!');

//                                            T1Store.load();
//                                        }
//                                    },
//                                    failure: function (response, options) {

//                                    }
//                                });
//                            }

//                    });
//                }
//            }
//        ]
//    });

//    var T1Form1 = Ext.widget({
//        xtype: 'form',
//        layout: 'form',
//        frame: false,
//        cls: 'T1b',
//        title: '',
//        autoScroll: true,
//        bodyPadding: '5 5 0',
//        fieldDefaults: {
//            labelAlign: 'right',
//            msgTarget: 'side',
//            labelWidth: 90
//        },
//        defaultType: 'textfield',
//        items: [
//            {
//                fieldLabel: 'Update',
//                name: 'x',
//                xtype: 'hidden'
//            },
//            {
//                xtype: 'displayfield',
//                fieldLabel: '申請單號',
//                name: 'DOCNO',
//                enforceMaxLength: true,
//                maxLength: 100,
//                readOnly: true,
//                submitValue: true
//            },{
//                xtype: 'displayfield',
//                fieldLabel: '申請日期',
//                name: 'APPTIME',
//                enforceMaxLength: true,
//                maxLength: 100,
//                readOnly: true,
//                submitValue: true
//            },{
//                xtype: 'displayfield',
//                fieldLabel: '申請人員',
//                name: 'APPID',
//                enforceMaxLength: true,
//                maxLength: 100,
//                readOnly: true,
//                submitValue: true
//            }, {
//                xtype: 'displayfield',
//                fieldLabel: '狀態',
//                name: 'FLOWID',
//                enforceMaxLength: true,
//                maxLength: 100,
//                readOnly: true,
//                submitValue: true

//            },{
//                xtype: 'displayfield',
//                fieldLabel: '申請單位',
//                name: 'INID_NAME',
//                enforceMaxLength: true,
//                maxLength: 100,
//                readOnly: true,
//                submitValue: true
//            },{
//                xtype: 'combo',
//                store: STKTRANSKINDStore,
//                displayField: 'TEXT',
//                valueField: 'VALUE',
//                queryMode: 'local',
//                fieldLabel: '異動類別',
//                name: 'STKTRANSKIND',
//                enforceMaxLength: true,
//                padding: '0 4 0 4',
//                submitValue: true,
//                width: 150
//            },

//        ],
//        buttons: [
//            {
//                itemId: 'submit', text: '儲存', hidden: true,
//                handler: function () {
//                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)


//                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
//                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
//                            if (btn === 'yes') {
//                                T1Submit1();
//                                T1Set = '';
//                            }
//                        });
//                    }
//                    else
//                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
//                }
//            },
//            {
//                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup1

//            }
//        ]
//    });
//    var T1Form = Ext.widget({
//        id:'t1form',
//        hidden: true,
//        xtype: 'form',
//        layout: 'form',
//        frame: false,
//        cls: 'T1b',
//        title: '',
//        autoScroll: true,
//        bodyPadding: '5 5 0',
//        fieldDefaults: {
//            labelAlign: 'right',
//            msgTarget: 'side',
//            labelWidth: 90
//        },
//        defaultType: 'textfield',
//        items: [{
//            name: 'x',
//            xtype: 'hidden'
//        }, {

//            fieldLabel: '申請單號',
//            readOnly: true,
//            name: 'DOCNO'
//            },
//            MMCode,
//            {
//            fieldLabel: "英文品名",
//            name: 'MMNAME_E',
//            style: 'text-align:left',
//            align: 'left',
//            width: 200,
//             readOnly: true
//        }, {
//             fieldLabel: "申請數量",
//             name: 'APPQTY',
//             style: 'text-align:left',
//             align: 'left',
//             width: 100
//        }, {
//            fieldLabel: "申請單位",
//            name: 'BASE_UNIT',
//            style: 'text-align:left',
//            align: 'left',
//            readOnly: true,
//            width: 100
//        }, {
//            xtype: 'combo',
//            fieldLabel: "補發原因",
//            name: 'GTAPL_RESON',
//            style: 'text-align:left',
//            align: 'left',
//            readOnly: true,
//            store: ReasonStore,
//            displayField: 'TEXT',
//            valueField: 'VALUE',
//            queryMode: 'local',
//            padding: '0 4 0 4',
//            width: 150

//        }, {
//            xtype: 'textareafield',
//            fieldLabel: '備註',
//            name: 'APLYITEM_NOTE',
//            enforceMaxLength: true,
//            maxLength: 100,
//            height: 200,
//            readOnly: true
//        }, {
//                fieldLabel:"病床號",
//                name: 'BEDNO',

//           style: 'text-align:left',
//           align: 'left',
//           readOnly: true,
//           id: 'BEDNO', 
//           hidden: true,
//           width: 100,
//           listeners: {
//               hide: function () {
//                   t1form.down('#con-BEDNO').setVisible(false);
//               },
//               show: function () {
//                   t1form.down('#con-BEDNO').setVisible(true);
//               }
//           }

//        }, {
//            fieldLabel:"病歷號",
//            name: 'MEDNO',
//            style: 'text-align:left',
//            align: 'left',
//            id: 'MEDNO', 
//            hidden: true,
//            width: 100,
//            listeners: {
//                hide: function () {
//                    t1form.down('#con-MEDNO').setVisible(false);
//                },
//                show: function () {
//                    t1form.down('#con-MEDNO').setVisible(true);
//                }
//            }
//        }, {
//            fieldLabel: "病患姓名",
//            name: 'CHINNAME',
//            style: 'text-align:left',
//            align: 'left',
//            readOnly: true,
//            id: 'CHINNAME', 
//            hidden: true,
//            width: 100,
//            listeners: {
//                hide: function () {
//                    t1form.down('#con-CHINNAME').setVisible(false);
//                },
//                show: function () {
//                    t1form.down('#con-CHINNAME').setVisible(true);
//                }
//            }
//        }, {
//            fieldLabel:"追藥時間",
//            name: 'ORDERDATE',
//            style: 'text-align:left',
//            align: 'left',
//            readOnly: true,
//            id: 'ORDERDATE', 
//            hidden: true,
//            width: 100,
//            listeners: {
//                hide: function () {
//                    t1form.down('#con-ORDERDATE').setVisible(false);
//                },
//                show: function () {
//                    t1form.down('#con-ORDERDATE').setVisible(true);
//                }
//            }
//        }, {
//            fieldLabel: "SEQ",
//            name: 'SEQ',
//            style: 'text-align:left',
//            align: 'left',
//            hidden: true,
//            width: 100

//        }
//        ],
//        buttons: [{
//            itemId: 'submit', text: '儲存', hidden: true,
//            handler: function () {
//                if (this.up('form').getForm().isValid()) {
//                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
//                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
//                        if (btn === 'yes') {
//                            T1Submit();
//                        }
//                    }
//                    );

//                }
//                /*else
//                    Ext.Msg.alert('提醒', '輸入資料格式有誤');*/
//            }
//        }, {
//            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
//        }]
//    });
//    function T1Submit1() {
//        var f = T1Form1.getForm();
//        if (f.isValid()) {
//            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
//            myMask.show();
//            f.submit({
//                url: T1Set,
//                success: function (form, action) {
//                    myMask.hide();
//                    var f2 = T1Form1.getForm();
//                    var r = f2.getRecord();
//                    switch (f2.findField("x").getValue()) {
//                        case "I":
//                            T1QueryForm.getForm().reset();
//                            var v = action.result.etts[0];
//                            T2Store.removeAll();
//                            /T1QueryForm.getForm().findField('P1').setValue(v.DOCNO);
//                            T1Load();
//                            msglabel('訊息區:資料新增成功');
//                            T1Set = '';
//                            break;
//                        case "U":
//                            T1Load();
//                            msglabel('訊息區:資料更新成功');
//                            T1Set = '';
//                            break;
//                        case "R":
//                            T1Load();
//                            msglabel('訊息區:資料退回成功');
//                            T1Set = '';
//                            break;
//                    }

//                    T1Cleanup1();
//                },
//                failure: function (form, action) {
//                    myMask.hide();
//                    switch (action.failureType) {
//                        case Ext.form.action.Action.CLIENT_INVALID:
//                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
//                            break;
//                        case Ext.form.action.Action.CONNECT_FAILURE:
//                            Ext.Msg.alert('失敗', 'Ajax communication failed');
//                            break;
//                        case Ext.form.action.Action.SERVER_INVALID:
//                            Ext.Msg.alert('失敗', action.result.msg);
//                            break;
//                    }
//                }
//            });
//        }
//    }


//    function T1Submit() {
//        var f = T1Form.getForm();
//        console.log(f)
//        if (f.isValid()) {
//            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
//            myMask.show();
//            f.submit({
//                url: T1Set,
//                success: function (form, action) {
//                    myMask.hide();
//                    var f2 = T1Form.getForm(); 
//                    var r = f2.getRecord();
//                    console.log(r)
//                    switch (f2.findField("x").getValue()) {
//                        case "I":
//                            T1QueryForm.getForm().reset();
//                            var v = action.result.etts[0];
//                            T1QueryForm.getForm().findField('P0').setValue(v.DOCNO);
//                            r.set(v);
//                            T2Store.insert(0, r);
//                            r.commit();
//                            msglabel('訊息區:資料新增成功');
//                            break;
//                        case "U":
//                            var v = action.result.etts[0];
//                            r.set(v);
//                            r.commit();
//                            msglabel('訊息區:資料修改成功');
//                            break;
//                        case "A":
//                            var v = action.result.etts[0];
//                            r.set(v);
//                            r.commit();
//                            msglabel('訊息區:資料核撥成功');
//                            break;
//                        case "D":
//                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
//                            r.commit();
//                            msglabel('訊息區:資料刪除成功');
//                            break;
//                    }
//                    T1Cleanup();
//                    T1Load();
//                    TATabs.setActiveTab('Query');
//                },
//                failure: function (form, action) {
//                    myMask.hide();
//                    switch (action.failureType) {
//                        case Ext.form.action.Action.CLIENT_INVALID:
//                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
//                            break;
//                        case Ext.form.action.Action.CONNECT_FAILURE:
//                            Ext.Msg.alert('失敗', 'Ajax communication failed');
//                            break;
//                        case Ext.form.action.Action.SERVER_INVALID:
//                            Ext.Msg.alert('失敗', action.result.msg);
//                            break;
//                    }
//                }
//            });
//        }
//    }


//    function T1Cleanup1() {
//        T1Set = '';
//        viewport.down('#t1Grid').unmask();
//        viewport.down('#t2Grid').unmask();
//        Ext.getCmp('eastform').collapse();
//        var f = T1Form1.getForm();
//        f.reset();
//        f.getFields().each(function (fc) {
//            if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
//                fc.setReadOnly(true);
//            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
//                fc.setReadOnly(true);
//            }
//        });
//        T1Form1.down('#cancel').hide();
//        T1Form1.down('#submit').hide();
//        f.findField('TOWH').setReadOnly(true);
//        f.findField('FRWH').setReadOnly(true);
//        viewport.down('#form').setTitle('瀏覽');
//        var currentTab = TATabs.getActiveTab();
//        currentTab.setTitle('瀏覽');
//        setFormT1a();
//    }

//    function T1Cleanup() {
//        viewport.down('#t2Grid').unmask();
//        var f = T1Form.getForm();
//        f.reset();
//        f.getFields().each(function (fc) {
//            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
//                fc.setReadOnly(true);
//            } else if (fc.xtype == "datefield") {
//                fc.readOnly = true;
//            }
//        });
//        T1Form.down('#cancel').hide();
//        T1Form.down('#submit').hide();
//        viewport.down('#form').setTitle('瀏覽');
//        T1QueryForm.getForm().findField('P2').setValue("1");
//        T1Form.setVisible(false);
//        T1QueryForm.setVisible(true);
//        TATabs.setActiveTab('Query');
//    }


//    var T2Tool = Ext.create('Ext.PagingToolbar', {
//        store: T2Store,
//        displayInfo: true,
//        border: false,
//        plain: true,
//        buttons: [
//            {
//                itemId: 'add', text: '新增',  handler: function () {
//                    T1Set = '../../../api/AB0013/CreateD';
//                    setFormT1('I', '新增');
//                }
//            },
//            {
//                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
//                    T1Set = '../../../api/AB0013/UpdateD';
//                    setFormT1("U", '修改');
//                }
//            },
//            {
//                itemId: 'delete', text: '刪除', disabled: true,
//                handler: function () {
//                    var selection = T2Grid.getSelection();
//                    if (selection.length === 0) {
//                        Ext.Msg.alert('提醒', '請勾選項目');
//                    }
//                    else {
//                        let name = '';
//                        let docno = '';
//                        let seq = '';
//                        selection.map(item => {
//                            name += '「' + item.get('SEQ') + '」<br>';
//                            docno += item.get('DOCNO') ;
//                            seq += item.get('SEQ') ;
//                        });
//                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
//                            if (btn === 'yes') {
//                                Ext.Ajax.request({
//                                    url: '/api/AB0013/DeleteD',
//                                    method: reqVal_p,
//                                    params: {
//                                        DOCNO: docno,
//                                        SEQ: seq
//                                    },
//                                    success: function (response) {
//                                        var data = Ext.decode(response.responseText);
//                                        if (data.success) {
//                                            Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
//                                            T2Store.removeAll();
//                                            T2Grid.getSelectionModel().deselectAll();
//                                            T2Load();
//                                            Ext.getCmp('btnSubmit').setDisabled(true);
//                                        }
//                                    },
//                                    failure: function (response) {
//                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
//                                    }
//                                });
//                            }
//                        }
//                        );
//                    }
//                }
//            }
//        ]
//    });

//    function setFormT1(x, t) {
//        viewport.down('#t2Grid').mask();
//         viewport.down('#t11Grid').mask();
//        viewport.down('#form').setTitle(t + T1Name);
//        viewport.down('#form').expand();
//        var f = T1Form.getForm();
//        if (kind!= "") {
//            Ext.getCmp("BEDNO").setVisible(false);
//            Ext.getCmp("MEDNO").setVisible(false);
//            Ext.getCmp("CHINNAME").setVisible(false);
//            Ext.getCmp("ORDERDATE").setVisible(false);


//        }
//        else {
//            Ext.getCmp("BEDNO").setVisible(false);
//            Ext.getCmp("MEDNO").setVisible(false);
//            Ext.getCmp("CHINNAME").setVisible(false);
//            Ext.getCmp("ORDERDATE").setVisible(false);

//            Ext.getCmp("t1form").doLayout();
//        }
//        if (x === "I") {
//            isNew = true;
//            alert(x);
//            var records = T1Grid.getSelectionModel().getSelection()[0];
//            var r = Ext.create('T1Model');
//            console.log(records)
//            T1Form.loadRecord(r);
//            f.findField('MMCODE').setReadOnly(false);
//            f.findField('APPQTY').setReadOnly(false);
//            f.findField('GTAPL_RESON').setReadOnly(false);
//            f.findField('APLYITEM_NOTE').setReadOnly(false);
//            f.findField('BASE_UNIT').setReadOnly(false);
//            f.findField('DOCNO').setValue(records.data.DOCNO);
//            f.findField('MMNAME_E').setValue(records.data.MMNAME_E);
//            f.findField('FRWH').setValue(records.data.FRWH);
//            T1Form.down('#cancel').setVisible(true);
//            T1Form.down('#submit').setVisible(true);
//            f.findField('LOT_NO').setReadOnly(false);
//            f.findField('PO_NO').setReadOnly(false);
//            // f.findField('PO_NO').setReadOnly(false);
//            f.findField('QTY').setReadOnly(false);

//            f.clearInvalid();
//            f.findField('INID').setValue(session['Inid'])
//            if (T1Form.hidden === true) {
//                T1QueryForm.setVisible(false);
//                T1Form1.setVisible(false)
//                T1Form.setVisible(true);
//                TATabs.setActiveTab('Form');
//            }
//        }
//        else {
//            u = f.findField('INID');
//        }
//        f.findField('x').setValue(x);
//        f.findField('MMCODE').setReadOnly(false);
//        f.findField('APPQTY').setReadOnly(false);
//        f.findField('GTAPL_RESON').setReadOnly(false);
//        f.findField('APLYITEM_NOTE').setReadOnly(false);
//        f.findField('STATUS').setReadOnly(false);
//        f.findField('PO_NO').setReadOnly(false);
//        f.findField('QTY').setReadOnly(false);
//        f.findField('MEMO').setReadOnly(false);
//        T1Form.down('#cancel').setVisible(true);
//        T1Form.down('#submit').setVisible(true);
//         u.focus();

//    }


//    var T1Grid = Ext.create('Ext.grid.Panel', {
//        menuDisabled: true,
//        store: T1Store,
//        plain: true,
//        loadingText: '處理中...',
//        loadMask: true,
//        cls: 'T1',
//        plugins: [
//            Ext.create("Ext.grid.plugin.CellEditing", {
//                //clicksToEdit: 1,
//            })
//        ],
//        dockedItems: [{
//            dock: 'top',
//            xtype: 'toolbar',
//            autoScroll: true,
//            items: [T1Tool]
//        }],

//        columns: [
//            { xtype: 'rownumberer' },
//            { text: '異動單號', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 140 },
//            { text: '狀態', dataIndex: 'FLOWID', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
//            { text: '申請日期', dataIndex: 'APPTIME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
//            { text: '申請庫別', dataIndex: 'FRWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
//            { text: '異動類別', dataIndex: 'STKTRANSKIND', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
//            { text: '藥品名稱', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 160 },
//            { text: '藥品名稱', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 160 },


//        ],
//        selModel: Ext.create('Ext.selection.CheckboxModel', {//check box
//            selectable: function (record) {
//                console.log(record)
//                return record.get('FLOWID') !== '申請中';
//            },
//        },
//        ),
//        listeners: {
//            click: {
//                element: 'el',
//                fn: function () {
//                if (T1Form1.hidden === true) {
//                    T1Form1.setVisible(true);
//                    T1Form.setVisible(false);
//                }
//                 grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
//                if (T1LastRec != null) {
//                    Ext.getCmp('btnUpdate').setDisabled(false);
//                    Ext.getCmp('btnDel').setDisabled(false);
//                    Ext.getCmp('btnSubmit').setDisabled(false);
//                    var records = T1Grid.getSelectionModel().getSelection();
//                    Ext.getCmp('t1edit').setDisabled(true);
//                    Ext.getCmp('delete').setDisabled(true);
//                    Ext.getCmp('add').setDisabled(false);
//                    Ext.getCmp('btnCopy').setDisabled(false);

//                    if (T1Set === '')
//                    Dno = record.data.DOCNO;
//                    kind = record.data.STKTRANSKIND
//                    alert("1")
//                        T1LastRec = records[0];
//                    setFormT1U();
//                    T2Load();

//                }
//            },
//            selectionchange: function (model, records) {
//                T1Rec = records.length;
//                T1LastRec = records[0];
//                console.log(records)
//                if (records.length > 0)
//                {
//                    Dno = records[0].data.DOCNO;
//                    kind = records[0].data.STKTRANSKIND
//                }
//                alert("S")
//                setFormT1U();
//                T2Load();

//                if (T1LastRec == null)
//                    Ext.getCmp('btnAdd2').setDisabled(true);
//            }
//        },
//    });

//    function setFormT1U() {



//        T1Grid.down('#btnUpdate').setDisabled(T1Rec === 0);
//        T1Grid.down('#btnDel').setDisabled(T1Rec === 0);
//         T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
//        if (T1LastRec != null) {
//            isNew = false;
//            console.log(T1LastRec);
//            T1Form1.loadRecord(T1LastRec);
//            var f = T1Form1.getForm();
//            alert("5")
//            T1Form1.setVisible(true);
//            T1QueryForm.setVisible(false);
//            var f = T1Form.getForm();
//            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
//            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
//            f.findField("INID_NAME").setValue(T1LastRec.data["FRWH"]);
//            f.findField("FLOWID").setValue(T1LastRec.data["FLOWID"]);
//            f.findField('BASE_UNIT').setValue(T1LastRec.data["BASE_UNIT"]);
//            f.findField('STKTRANSKIND').setValue(T1LastRec.data["STKTRANSKIND"]);
//            T1Form1.loadRecord(T1LastRec);
//            T1Grid.down('#btnUpdate').setDisabled(false);
//            T1Grid.down('#btnDel').setDisabled(false);


//        }
//        else {
//            alert("3")
//            T1Form1.getForm().reset();
//            T1QueryForm.setVisible(true);
//            T1Form1.setVisible(false);
//            TATabs.setActiveTab('Form');
//        }
//        alert(T1Form.hidden)



//    }

//    function setFormT1F(x, t) {
//        function setFormT1(x, t) {
//            viewport.down('#t1Grid').mask();
//            viewport.down('#t2Grid').mask();
//            viewport.down('#form').setTitle(t);
//            viewport.down('#form').expand();
//            TATabs.setActiveTab('Form');
//            var currentTab = TATabs.getActiveTab();
//            currentTab.setTitle('填寫');

//            var f = T1Form1.getForm();
//            if (x === "I") {
//                isNew = true;
//                var r = Ext.create('T1Model'); // /Scripts/app/model/MiUnitexch.js
//                T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容

//                f.findField('FLOWID').setVisible(false);
//                f.findField('FRWH').setVisible(false);
//                f.findField("DOCNO").setValue('系統自編');
//                f.findField("APPID").setValue(userId + ' ' + userName);
//                f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);
//                f.findField("APPTIME").setValue(getChtToday());
//            }
//            else {
//                console.log(T1LastRec)
//                f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
//                f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
//                f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
//                f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
//                f.findField("FLOWID").setValue(T1LastRec.data["FLOWID"]);



//            }
//            f.findField('x').setValue(x);
//            f.findField('MAT_CLASS').setReadOnly(false);
//            T1Form1.down('#cancel').setVisible(true);
//            T1Form1.down('#submit').setVisible(true);
//            u.focus();
//        }



//    function setFormT1a() {

//        T2Grid.down('#t1edit').setDisabled(T1Rec === 0);
//        T2Grid.down('#delete').setDisabled(T1Rec === 0);
//         T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
//        if (T1LastRec) {
//            isNew = false;
//            console.log(T1LastRec);
//            T1Form.loadRecord(T1LastRec);
//            var f = T1Form1.getForm();

//            T2Grid.down('#t1edit').setDisabled(false);
//            T2Grid.down('#delete').setDisabled(false);

//        }
//        else {
//            T1Form.getForm().reset();

//        }
//        alert("6")
//        if (T1Form1.hidden === true) {
//            T1QueryForm.setVisible(false);
//            T1Form.setVisible(true);
//            TATabs.setActiveTab('Form');
//        }
//    }

//    var T2Grid = Ext.create('Ext.grid.Panel', {
//        title: 'Dno',
//        menuDisabled: true,
//        store: T2Store,
//        plain: true,
//        loadingText: '處理中...',
//        loadMask: true,
//        cls: 'T1',
//        plugins: [
//            Ext.create("Ext.grid.plugin.CellEditing", {
//                clicksToEdit: 1,
//            })
//        ],
//        dockedItems: [{
//            dock: 'top',
//            xtype: 'toolbar',
//            autoScroll: true,
//            items: [T2Tool]
//        }],
//        columns: [
//            { xtype: 'rownumberer' },
//            { text: '申請庫別', dataIndex: 'FRWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
//            { text: '銷帳庫別', dataIndex: 'FRWH_D', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80, },

//            {
//                text: '院內碼', dataIndex: 'MMCODE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80,


//            },
//            { text: '藥品名稱', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, width: 160 },

//            { text: 'QID', dataIndex: 'QID',align: 'center', style: 'text-align:center', width: 40, sortable: true },
//            {
//                text: '補發數量', dataIndex: 'APPQTY', align: 'right', style: 'text-align:left', fontColor: 'red', menuDisabled: true, width: 80, 
//            },
//            { text: '單位', dataIndex: 'BASE_UNIT', align: 'right', style: 'text-align:left', menuDisabled: true, width: 60 },
//            { text: '補發原因', dataIndex: 'GTAPL_RESON', align: 'right', style: 'text-align:left', menuDisabled: true, width: 60 },
//            { text: '備註', dataIndex: 'APLYITEM_NOTE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
//            { text: '病床號', dataIndex: 'BEDNO', align: 'left', style: 'text-align:left', menuDisabled: true, hide: true, width: 80 },
//            { text: '病歷號', dataIndex: 'MEDNO', align: 'left', style: 'text-align:left', menuDisabled: true, hide: true, width: 80 },
//            { text: '病患姓名', dataIndex: 'CHINNAME', align: 'left', style: 'text-align:left', menuDisabled: true, hide: true, width: 80 },
//            { text: '追藥時間', dataIndex: 'ORDERDATE', align: 'left', style: 'text-align:left', menuDisabled: true, hide: true, width: 80 },
//            { text: 'DOCNO', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },    
//            { text: 'SEQ', dataIndex: 'SEQ', align: 'left', style: 'text-align:left', menuDisabled: true, hidden: true, width: 80 },    
//        selModel: Ext.create('Ext.selection.CheckboxModel'),//check box
//         {
//            header: "",
//            flex: 1
//        }],
//        listeners: {
//            click: {
//                element: 'el',
//                fn: function () {
//                    if (T1Form.hidden === true) {
//                        T1QueryForm.setVisible(false);
//                        T1Form.setVisible(true);
//                        T1Form1.setVisible(false);
//                        TATabs.setActiveTab('Form');
//                    }
//                }
//            },
//            selectionchange: function (model, records) {
//                T1Rec = records.length;
//                T1LastRec = records[0];
//                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
//                ComboData();
//                setFormT1a();
//                T11Load();
//            }
//        }

//    });

//    function T1Load() {
//        T1Tool.moveFirst();
//        T1Store.load({
//            params: {
//                start: 0
//            }
//        });
//    }

//    function T2Load() {
//        T2Store.load({
//            params: {
//                p0: Dno,
//                p1: kind,
//            }
//        });
//    }

//    var TATabs = Ext.widget('tabpanel', {
//        plain: true,
//        border: false,
//        resizeTabs: true,
//        layout: 'fit',
//        defaults: {
//            layout: 'fit'
//        },
//        items: [{
//            itemId: 'Query',
//            title: '查詢',
//            items: [T1QueryForm]
//        }, {
//            itemId: 'Form',
//            title: '瀏覽',
//            items: [T1Form1,T1Form]
//        }]
//    });

//    var viewport = Ext.create('Ext.Viewport', {
//        id: 'viewport',
//        renderTo: body,
//        layout: {
//            type: 'border',
//            padding: 0
//        },
//        defaults: {
//            split: true
//        },
//        items: [{
//            itemId: 't1Grid',
//            region: 'center',
//            layout: 'fit',
//            collapsible: false,
//            title: '',
//            border: false,
//            items: [{
//                region: 'center',
//                layout: {
//                    type: 'border',
//                    padding: 0
//                },
//                collapsible: false,
//                title: '',
//                split: true,
//                width: '80%',
//                flex: 1,
//                minWidth: 50,
//                minHeight: 140,
//                items: [
//                    {
//                        region: 'north',
//                        layout: 'fit',
//                        collapsible: false,
//                        title: '',
//                        split: true,
//                        height: '50%',
//                        items: [T1Grid]
//                    },
//                    {
//                        itemId: 't2Grid',
//                        region: 'center',
//                        layout: 'fit',
//                        collapsible: false,
//                        title: '',
//                        height: '50%',
//                        split: true,
//                        items: [TATabs]
//                        items: [T2Grid]
//                    }
//                ]
//            }]
//        },
//        {
//            itemId: 'form',
//            id: 'eastform',
//            region: 'east',
//            collapsible: true,
//            floatable: true,
//            width: 300,
//            title: '',
//            border: false,
//            layout: {
//                type: 'fit',
//                padding: 5,
//                align: 'stretch'
//            },
//            items: [TATabs]
//        }
//        ]
//    });
//    getWhnoCombo();
//    UserInfoLoad();

//    Ext.getCmp('btnUpdate').setDisabled(true);
//    Ext.getCmp('btnDel').setDisabled(true);
//    Ext.getCmp('btnSubmit').setDisabled(true);
//    Ext.getCmp('btnSubmit').setDisabled(true);
//});