Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "中藥訂購作業";
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var T1Rec = 0;
    var T1LastRec = null;
    var T3LastRec = null;
    var tc_type = 'A'; //A: 科學中藥 B:飲片
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    var viewModel = Ext.create('WEBAPP.store.GA0002VM');

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var mLabelWidth = 90;

    //#region CheckboxModel
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
    //#endregion

    //#region store
    var tcTypeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { VALUE: '', TEXT: '全部' },
            { VALUE: 'A', TEXT: 'A 科學中藥' },
            { VALUE: 'B', TEXT: 'B 飲片' }
        ]
    });

    var T1Store = viewModel.getStore('Master');
    function T1Load() {
        T2Store.removeAll();
        T1LastRec = null;
        Ext.getCmp('masterUpdate').disable();
        Ext.getCmp('masterDelete').disable();
        Ext.getCmp('placeOrder').disable();

        T1Store.getProxy().setExtraParam("pur_no", T1Query.getForm().findField('P0').getValue());
        //T1Store.getProxy().setExtraParam("tc_type", tc_type);
        T1Store.getProxy().setExtraParam("tc_type", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("start_date", T1Query.getForm().findField('P2').rawValue);
        T1Store.getProxy().setExtraParam("end_date", T1Query.getForm().findField('P3').rawValue);
        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('Details');
    function T2Load() {
        T2Store.getProxy().setExtraParam("pur_no", T1LastRec.data.PUR_NO);

        T2Tool.moveFirst();
    }

    var T31Store = viewModel.getStore('Invqmtrs');
    function T31Load() {
        T31Store.getProxy().setExtraParam("pur_no", T1LastRec.data.PUR_NO);
        T31Store.getProxy().setExtraParam("tc_type", T1LastRec.data.TC_TYPE);
        T31Store.getProxy().setExtraParam("inv_day", T31Query.getForm().findField('P3P0').getValue());
        T31Store.getProxy().setExtraParam("mmcode", T31Query.getForm().findField('P3P1').getValue());

        T31Tool.moveFirst();
    }

    var T41Store = viewModel.getStore('Orders');
    function T41Load() {
        T42Store.removeAll();

        T41Store.getProxy().setExtraParam("pur_no", T41Query.getForm().findField('P41P0').getValue());
        T41Store.getProxy().setExtraParam("tc_type", T41Query.getForm().findField('P41P1').getValue());
        T41Store.getProxy().setExtraParam("start_date", T41Query.getForm().findField('P41P2').rawValue);
        T41Store.getProxy().setExtraParam("end_date", T41Query.getForm().findField('P41P3').rawValue);

        T41Tool.moveFirst();
    }
    var T42Store = viewModel.getStore('Details');
    function T42Load(pur_no) {
        T42Store.getProxy().setExtraParam("pur_no", pur_no);

        T42Tool.moveFirst();
    }
    //#endregion

    //#region T1Query

    function clearMsg() {
        msglabel('');
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '訂單編號',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13,          // 可輸入最大長度為100
                    padding: '0 4 0 4',
                    width: 285
                },
                //{
                //    xtype: 'radiogroup',
                //    name: 'P1',
                //    fieldLabel: '藥品種類',
                //    items: [
                //        { boxLabel: '科學中藥', name: 'P1', inputValue: 'A', width: 100, checked: true },
                //        { boxLabel: '飲片', name: 'P1', inputValue: 'B', width: 100 }
                //    ],
                //    listeners: {
                //        change: function (field, newValue, oldValue) {
                //            changeTcType(newValue['P1']);
                //        }
                //    },                    
                //},
                {
                    xtype: 'combo',
                    store: tcTypeStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '藥品種類',
                    name: 'P1',
                    id: 'P1',
                    labelWidth: mLabelWidth,
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 180,
                    value: ''

                },
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',            
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '訂購日期',
                    name: 'P2',
                    id: 'P2',
                    labelWidth: mLabelWidth,
                    width: 180,
                    allowBlank: true,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P3',
                    id: 'P3',
                    labelWidth: 8,
                    labelSeparator: '',
                    width: 98,
                    allowBlank: true,
                    padding: '0 4 0 4'
                },
                {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    T1Load();
                    msglabel('');
                }
            }, {
                xtype: 'button',
                text: '清除',                
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('');
                }
            }

            ]
        }]
    });
    function changeTcType(value) {
        tc_type = value;
    }

    //#endregion

    //#region T1
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '新增',
                id: 'masterAdd',
                name: 'masterAdd',
                handler: function () {
                    T1Set = '/api/GA0002/MasterInsert';
                    setFormT1('I', '新增');
                    clearMsg();
                }
            },
            {
                text: '修改',
                id: 'masterUpdate',
                name: 'masterUpdate',
                disabled:true,
                handler: function () {
                    if (T1LastRec == null) {
                        Ext.Msg.alert('提醒', '請選擇要修改的項目');
                        return;
                    }

                    T1Set = '/api/GA0002/MasterUpdate';
                    setFormT1('U', '修改');
                    clearMsg();
                }
            },
            {
                text: '刪除',
                id: 'masterDelete',
                name: 'masterDelete',
                disabled:true,
                handler: function () {
                    var selection = T1Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要刪除的項目');
                        return;
                    }

                    var pur_nos = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (pur_nos != '') {
                            pur_nos += ','
                        }
                        //pur_nos += ("'" + selection[i].data.PUR_NO + "'");
                        pur_nos += selection[i].data.PUR_NO;
                    }

                    Ext.MessageBox.confirm('刪除', '刪除主檔會將明細一併刪除，是否確定刪除? ', function (btn, text) {
                        if (btn === 'yes') {
                            masterDelete(pur_nos);
                        }
                    });
                }
            },
            {
                text: '訂購',
                id: 'placeOrder',
                name: 'placeOrder',
                disabled:true,
                handler: function () {
                    var selection = T1Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要訂購的項目');
                        return;
                    }

                    Ext.MessageBox.confirm('提示', '是否確定訂購? ', function (btn, text) {
                        if (btn === 'yes') {
                            var pur_nos = '';
                            for (var i = 0; i < selection.length; i++) {
                                if (pur_nos != '') {
                                    pur_nos += ','
                                }
                                //pur_nos += ("'" + selection[i].data.PUR_NO + "'");
                                pur_nos += selection[i].data.PUR_NO;
                            }

                            placeOrder(pur_nos);
                        }
                    });

                    
                }
            },
            {
                text: '取消訂購',
                id: 'showOrderWindow',
                name: 'showOrderWindow',
                handler: function () {
                    orderWindow.show();
                    T41Store.removeAll();
                    T42Store.removeAll();

                    Ext.getCmp('cancelOrder').disable();
                }
            },
            {
                text: '查詢訂單',
                id: 'queryOrder',
                name: 'queryOrder',
                handler: function () {
                   // parent.link2('/Form/Index/GA0005', ' 中藥訂單查詢(GA0005)', true);
                    popWinForm('GA0005');
                }
            },
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "訂單編號",
                dataIndex: 'PUR_NO',
                width: 150
            },
            {
                text: "訂購日期",
                dataIndex: 'PUR_DATE',
                width: 100
            },
            {
                text: "藥品種類",
                dataIndex: 'TC_TYPE',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data.TC_TYPE == 'A') {
                        return 'A 科學中藥';
                    } 
                    return 'B 飲片';
                },
            },
            {
                text: "訂購人",
                dataIndex: 'PUR_UNM_NAME',
                width: 100
            },
            {
                text: "訂購單備註",
                dataIndex: 'PUR_NOTE',
                width: 130,
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;
                setFormT1a();
                if (T1LastRec != null) {
                    Ext.getCmp('masterUpdate').enable();
                    Ext.getCmp('masterDelete').enable();
                    Ext.getCmp('placeOrder').enable();
                } else {
                    Ext.getCmp('masterUpdate').disable();
                    Ext.getCmp('masterDelete').disable();
                    Ext.getCmp('placeOrder').disable();
                }

                clearMsg();
                T2Load();

                Ext.getCmp('detailDelete').disable();
                var selection = T1Grid.getSelection();
                if (selection.length > 1) {
                    Ext.getCmp('masterUpdate').disable();
                    Ext.getCmp('detailAdd').disable();
                } else if (selection.length == 1) {
                    Ext.getCmp('masterUpdate').enable();
                    Ext.getCmp('masterDelete').enable();
                    Ext.getCmp('placeOrder').enable();
                    Ext.getCmp('detailAdd').enable();
                } else {
                    Ext.getCmp('masterUpdate').enable();
                    Ext.getCmp('detailAdd').enable();
                    T2Store.removeAll();
                }
            },  
            selectionchange: function (model, records) {
                Ext.getCmp('detailDelete').disable();
                if (records.length == 0) {
                    T1LastRec = null;
                    T1Cleanup()
                    Ext.getCmp('masterUpdate').disable();
                    Ext.getCmp('masterDelete').disable();
                    Ext.getCmp('placeOrder').disable();

                    T2Store.removeAll();
                    Ext.getCmp('detailAdd').disable();
                    Ext.getCmp('detailDelete').disable();
                    return;
                }

                if (records.length > 1) {
                    Ext.getCmp('masterUpdate').disable();
                    Ext.getCmp('detailAdd').disable();
                    T2Store.removeAll();
                } else if (records.length == 1) {
                    Ext.getCmp('masterUpdate').enable();
                    Ext.getCmp('masterDelete').enable();
                    Ext.getCmp('placeOrder').enable();
                    Ext.getCmp('detailAdd').enable();
                } else {
                    Ext.getCmp('masterUpdate').enable();
                    Ext.getCmp('detailAdd').enable();
                    T2Store.removeAll();
                }

                clearMsg();
                
            }
        }
    });

    function masterDelete(pur_nos) {
        Ext.Ajax.request({
            url: '/api/GA0002/MasterDelete',
            method: reqVal_p,
            params: {
                pur_nos: pur_nos
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("刪除成功");
                    T1Load();
                    T1Cleanup();
                    T2Store.removeAll();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function placeOrder(pur_nos) {
        Ext.Ajax.request({
            url: '/api/GA0002/PlaceOrder',
            method: reqVal_p,
            params: {
                pur_nos: pur_nos
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("訂購成功");
                    T1Load(false);
                    T1Cleanup();
                    T2Store.removeAll();
                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var callableWin = null;
    popWinForm = function (url) {
        var strUrl =url;
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
                        if (url == 'GA0004') {
                            T31Load();
                        } 
                    }
                }]
            });
            var title = url == 'GA0005' ? '中藥訂單查詢' : '單位劑量轉換維護';
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion

    //#region T2
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '新增',
                id: 'detailAdd',
                name: 'detailAdd',
                disabled:true,
                handler: function () {
                    if (T1LastRec == null) {
                        Ext.Msg.alert('提醒', '請選擇要新增明細的訂單');
                        return;
                    }

                    detailInsertWindow.setTitle('明細選擇 訂單編號：' + T1LastRec.data.PUR_NO + '，藥品種類：' + T1LastRec.data.TC_TYPE_NAME);
                    detailInsertWindow.show();
                    T31Load();
                }
            },
            {
                text: '刪除',
                id: 'detailDelete',
                disabled: true,
                name: 'detailDelete',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要刪除的項目');
                        return;
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var item = {
                            PUR_NO: selection[i].data.PUR_NO,
                            MMCODE: selection[i].data.MMCODE,
                            AGEN_NAMEC: selection[i].data.AGEN_NAMEC
                        }
                        list.push(item);
                    }

                    Ext.MessageBox.confirm('刪除', '是否確定刪除? ', function (btn, text) {
                        if (btn === 'yes') {
                            detailDelete(list);
                        }
                    });
                }
            }
        ]
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "電腦編號",
                dataIndex: 'MMCODE',
                width: 80,
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                width: 120
            },
            {
                text: "藥商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 120,
            },
            {
                text: "訂購量",
                dataIndex: 'PUR_QTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "庫存天數",
                dataIndex: 'INV_DAY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單位劑量",
                dataIndex: 'PUR_UNIT',
                width: 90
            },
            {
                text: "進貨單價",
                dataIndex: 'IN_PURPRICE',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "金額小計",
                dataIndex: 'PUR_AMOUNT',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                Ext.getCmp('masterUpdate').disable();
                Ext.getCmp('masterDelete').disable();
                Ext.getCmp('placeOrder').disable();
            },
            selectionchange: function (model, records) {

                if (records.length == 0) {
                    Ext.getCmp('detailDelete').disable();
                    return;
                }

                T2LastRec = records[0];                
                if (T1LastRec != null) {
                    Ext.getCmp('detailDelete').enable();
                }
            }
        }
    });

    function detailDelete(list) {
        Ext.Ajax.request({
            url: '/api/GA0002/DetailDelete',
            method: reqVal_p,
            params: {
                item_string: Ext.util.JSON.encode(list)
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("刪除成功");
                    T2Load(false);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    //#endregion

    //#region T1Form
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
                fieldLabel: '訂單編號',
                name: 'PUR_NO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'datefield',
                fieldLabel: '訂購日期',
                name: 'PUR_DATE',
                id: 'PUR_DATE',
                enforceMaxLength: true,
                readOnly: true,
                minValue: new Date(),
                submitValue: true,
                hidden: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '訂購日期',
                name: 'PUR_DATE_DISPLAY',
                id:'PUR_DATE_DISPLAY',
                
                submitValue: true

            },
            {
                xtype: 'radiogroup',
                name: 'TC_TYPE',
                fieldLabel: '藥品種類',
                disabled: true,
                id:'TC_TYPE_RADIO',
                items: [
                    { boxLabel: '科學中藥', name: 'TC_TYPE', inputValue: 'A', width: 100, checked: true},
                    { boxLabel: '飲片', name: 'TC_TYPE', inputValue: 'B', width: 100 }
                ]
            },
            {
                xtype: 'displayfield',
                name: 'TC_TYPE_NAME',
                id: 'TC_TYPE_NAME',
                fieldLabel: '藥品種類',
            },
            {
                xtype: 'displayfield',
                fieldLabel: '訂購人',
                name: 'PUR_UNM',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            //{
            //    xtype: 'displayfield',
            //    fieldLabel: '訂單狀態',
            //    name: 'PURCH_ST',
            //    enforceMaxLength: true,
            //    readOnly: true,
            //    submitValue: true
            //},
            {
                xtype: 'textareafield',
                fieldLabel: '訂購單備註',
                name: 'PUR_NOTE',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();

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

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        f.findField('x').setValue(x);

        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.TC_PURCH_M'); // /Scripts/app/model/MiWexpinv.js
            T1Form.loadRecord(r);

            f.findField('PUR_NO').setValue('系統自編');
            f.findField('PUR_DATE').show();
            f.findField('PUR_DATE_DISPLAY').hide();
            f.findField('PUR_DATE').setValue(new Date());
            f.findField('PUR_DATE').setReadOnly(false);
            f.findField('TC_TYPE').setValue({ TC_TYPE: 'A' });
            tc_type = 'A';
            f.findField('TC_TYPE').enable();
            Ext.getCmp('TC_TYPE_RADIO').show();
            Ext.getCmp('TC_TYPE_NAME').hide();
            f.findField('PUR_UNM').setValue(userId + ' ' + userName);
            
            f.findField('PUR_NOTE').setValue('');
            f.findField('PUR_NOTE').setReadOnly(false);
        } else {
            T1Form.loadRecord(T1LastRec);


            //f.findField('PUR_NO').setValue(T1LastRed.data.PUR_NO);
            //f.findField('PUR_DATE').setValue(T1LastRec.data.PUR_DATE);
            //f.findField('PUR_DATE').setReadOnly(true);
            f.findField('PUR_DATE').hide();
            f.findField('PUR_DATE_DISPLAY').show();
            f.findField('TC_TYPE').setValue({ TC_TYPE: T1LastRec.data.TC_TYPE });
            Ext.getCmp('TC_TYPE_RADIO').hide();
            Ext.getCmp('TC_TYPE_NAME').show();
            f.findField('TC_TYPE_NAME').setValue(T1LastRec.data.TC_TYPE_NAME);
           // f.findField('PUR_UNM').setValue(T1LastRec.data.PUR_UNM);
           // f.findField('PUR_NOTE').setValue('');
            f.findField('PUR_NOTE').setReadOnly(false);
        }


        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);

    }
    function setFormT1a() {
        if (T1LastRec) {
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');

            f.findField('PUR_DATE').setReadOnly(true);
            f.findField('PUR_DATE').hide();
            f.findField('PUR_DATE_DISPLAY').show();

            f.findField('PUR_UNM').setValue(T1LastRec.data.PUR_UNM + ' ' + T1LastRec.data.PUR_UNM_NAME)

            Ext.getCmp('TC_TYPE_RADIO').hide();
            Ext.getCmp('TC_TYPE_NAME').show();

            f.findField('PUR_NOTE').setReadOnly(true);
        }
        else {
            T1Form.getForm().reset();
        }
    }
    function T1Submit() {
        var f = T1Form.getForm();
        var x = f.findField('x').getValue();
        if (x === 'I') {
            masterInsert();
            return;
        } else if (x === 'U') {
            masterUpdate();
            return;
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "numberfield" || fc.xtype == "combo") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
        Ext.getCmp('eastform').collapse();
    }

    function masterInsert() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        var f = T1Form.getForm();
        Ext.Ajax.request({
            url: '/api/GA0002/MasterInsert',
            method: reqVal_p,
            params: {
                pur_date: f.findField('PUR_DATE').rawValue,
                tc_type: f.findField('TC_TYPE').getValue()['TC_TYPE'],
                pur_note: f.findField('PUR_NOTE').getValue()
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("新增成功");
                    //tc_type = f.findField('TC_TYPE').getValue()['TC_TYPE'];
                    T1Query.getForm().findField('P0').setValue(data.msg);
                    T1Query.getForm().findField('P1').setValue(f.findField('TC_TYPE').getValue()['TC_TYPE']) ;
                    T1Load(false);
                    T1Cleanup();
                }
            },
            failure: function (response, options) {
                //Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
            }
        });
    }

    function masterUpdate() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        var f = T1Form.getForm();
        Ext.Ajax.request({
            url: '/api/GA0002/MasterUpdate',
            method: reqVal_p,
            params: {
                pur_no: f.findField('PUR_NO').getValue(),
                pur_date: f.findField('PUR_DATE').rawValue,
                tc_type: f.findField('TC_TYPE').getValue()['TC_TYPE'],
                pur_note: f.findField('PUR_NOTE').getValue()
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("新增成功");
                    T1Load(false);
                    T1Cleanup();
                }
            },
            failure: function (response, options) {
                //Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
            }
        });
    }
    //#endregion

    //#region detailInsertWindow
    var T31CellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1,
        autoCancel: true,
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                if (context.record.data.IS_VALID == 'Y') {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
    });
    var T31Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP21',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'numberfield',
                            fieldLabel: '庫存量天數<=',
                            labelAlign: 'right',
                            name: 'P3P0',
                            enforceMaxLength: false,
                            maxLength: 14,
                            hideTrigger: true,
                            width: 150,
                            labelSeparator: '',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '天',
                            labelSeparator: '',
                            width: 30

                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '藥品名稱',
                            name: 'P3P1',
                            id: 'P3P1',
                            labelAlign: 'right',
                            enforceMaxLength: false,
                            maxLength: 100,
                            labelWidth: 60,
                            width: 160,
                            padding: '0 4 0 4'
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                T31Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                this.up('form').getForm().reset();
                            }
                        }
                    ]
                },
            ]
        }]
    });
    var T31Tool = Ext.create('Ext.PagingToolbar', {
        store: T31Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '轉入明細',
                id: 'transferT31',
                name: 'transferT31',
                disabled:true,
                handler: function () {
                    var selection = T31Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.MessageBox.alert('提示', '請選擇要轉入明細的資料');
                        return;
                    }

                    var list = []; 
                    for (var i = 0; i < selection.length; i++) {
                        var reg = new RegExp("^([1-9][0-9]*|0)$");
                        if (selection[i].data.PUR_QTY == "0" || selection[i].data.PUR_QTY == "") {
                            var msg = selection[i].data.MMCODE + ' ' + selection[i].data.MMNAME_C + ' ' + selection[i].data.AGEN_NAMEC + '<br><span style="color:red">未填訂購量</span>';
                            Ext.MessageBox.alert('提示', msg);
                            return;
                        }
                        if (reg.test(selection[i].data.PUR_QTY) == false) {
                            var msg = selection[i].data.MMCODE + ' ' + selection[i].data.MMNAME_C + ' ' + selection[i].data.AGEN_NAMEC + '<br><span style="color:red">不可填入小數點</span>';
                            Ext.MessageBox.alert('提示', msg);
                            return;
                        }

                        var data = {
                            PUR_NO:T1LastRec.data.PUR_NO,
                            MMCODE: selection[i].data.MMCODE,
                            AGEN_NAMEC: selection[i].data.AGEN_NAMEC,
                            MMNAME_C: selection[i].data.MMNAME_C,
                            PUR_QTY: selection[i].data.PUR_QTY,
                            PUR_UNIT: selection[i].data.PUR_UNIT,
                            IN_PURPRICE: selection[i].data.IN_PURPRICE,
                            PUR_AMOUNT: Number(selection[i].data.PUR_QTY) * Number(selection[i].data.IN_PURPRICE)
                        };
                        list.push(data);
                    }

                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/GA0002/Transfer',
                        method: reqVal_p,
                        params: {
                            pur_no: T1LastRec.data.PUR_NO,
                            item_string: Ext.util.JSON.encode(list)
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);

                            myMask.hide();
                            if (data.success) {
                                msglabel('轉入明細完成');
                                T2Load();
                                T31Load();
                            }
                        },
                        failure: function (response, options) {
                            myMask.hide();
                            Ext.MessageBox.alert('提示', '發生例外錯誤');
                        }
                    });
                }
            },
            {
                text: '單位劑量轉換率維護',
                id: 'ga0004',
                name: 'ga0004',
                handler: function () {
                   // parent.link2('/Form/Index/GA0004', ' 單位劑量轉換率維護(GA0004)', true);
                    popWinForm('GA0004');
                }
            },
        ]
    });
    var T31Grid = Ext.create('Ext.grid.Panel', {
        store: T31Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        plugins: [T31CellEditing],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return record.data.IS_VALID == 'N';
            }
        }),
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        selType: 'checkboxmodel',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T31Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T31Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "資料年月",
                dataIndex: 'DATA_YM',
                width: 80,
                tooltip: 'test',
                tooltipType: 'qtip'
            },
            {
                text: "電腦編號",
                dataIndex: 'MMCODE',
                width: 70
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                width: 120
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70
            },

            {
                text: "前6個月平均消耗量",
                dataIndex: 'M6AVG_USEQTY',
                width: 130,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "庫存天數",
                dataIndex: 'INV_DAY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "應訂購量",
                dataIndex: 'EXP_PURQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "藥商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 120
            },
            {
                text: "單位劑量",
                dataIndex: 'PUR_UNIT',
                width: 70
            },
            {
                text: "建議訂購量",
                dataIndex: 'RCM_PURQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "訂購量",
                dataIndex: 'PUR_QTY',
                width: 100,
                align: 'right',
                style: 'text-align:left; color:red',
                editor: {
                    xtype: 'numberfield',
                    regexText: '只能輸入數字',
                    //regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    //maskRe: /[0-9]/,
                    //regex: /^([1-9][0-9]*|0)$/,
                    minValue: 1,
                    hideTrigger: true,
                    decimals: 0,
                    listeners: {
                        blur: function (self, record, event, eOpts) {
                            var index = -1;
                            var store = T31Grid.getStore().data.items;
                            for (var i = 0; i < store.length; i++) {
                                if (store[i] == T3LastRec) {
                                    index = i;
                                }
                            }
                            
                            T31Grid.getSelectionModel().deselect(index, true);

                            var reg = new RegExp("^([1-9][0-9]*|0)$");
                            if (isNaN(Number(self.value)) || Number(self.value) == 0) {
                                self.setValue('');
                                T3LastRec.set('PUR_QTY', '');
                                T3LastRec.set('PUR_DAY', null);
                                return;
                            }
                            if (reg.test(self.value) == false) {
                                self.setValue('');
                                T3LastRec.set('PUR_QTY', '');
                                T3LastRec.set('PUR_DAY', null);
                                setTimeout(function () {
                                    Ext.MessageBox.alert('提示', '<span style="color:red">不可填入小數點</span>');

                                }, 50)
                                return;
                            }
                            T3LastRec.set('PUR_QTY', self.value);
                            //T3LastRec.data.PUR_QTY = self.value;
                            
                            if (T3LastRec.data.M6AVG_USEQTY != null && T3LastRec.data.M6AVG_USEQTY != undefined && Number(T3LastRec.data.M6AVG_USEQTY) != 0) {
                                var temp = Math.round((Number(T3LastRec.data.PUR_QTY) / Number(T3LastRec.data.PURUN_MULTI) * Number(T3LastRec.data.BASEUN_MULTI)) / Number(T3LastRec.data.M6AVG_USEQTY));
                                // T3LastRec.data.PUR_DAY = Math.round((Number(T3LastRec.data.PUR_QTY) / Number(T3LastRec.data.PURUN_MULTI) * Number(T3LastRec.data.BASEUN_MULTI)) / Number(T3LastRec.data.M6AVG_USEQTY));
                                T3LastRec.set('PUR_DAY', temp);
                            } else {
                                temp = Math.round((Number(T3LastRec.data.PUR_QTY) / Number(T3LastRec.data.PURUN_MULTI) * Number(T3LastRec.data.BASEUN_MULTI)));
                                T3LastRec.set('PUR_DAY', temp);
                            }

                            T31Grid.getSelectionModel().select(index, true);

                            var selection = T31Grid.getSelection();
                            if (selection.length == 0) {
                                Ext.getCmp('transferT31').disable();
                            }

                            //T31Grid.getSelectionModel().deselect(rowIndex, true);
                            //T31Grid.getSelectionModel().select(rowIndex, true);
                        }
                    }
                }
            },
            {
                text: "訂購天數",
                dataIndex: 'PUR_DAY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                Ext.getCmp('transferT31').disable();
                if (records.length > 0) {
                    Ext.getCmp('transferT31').enable();
                    
                }
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                T3LastRec = record;
               
                //var columns = T31Grid.getColumns();
                //var index = getColumnIndex(columns, 'PUR_QTY');
                //if (index != cellIndex) {
                //    
                //    if (T3LastRec.data.PUR_QTY == null || T3LastRec.data.PUR_QTY == '') {
                //        T31Grid.getSelectionModel().deselect(index, true);
                //    }
                //    return;
                //}
                
                ////var store = T31Grid.getStore().data.items;
                ////var selection = T31Grid.getSelection()
                ////for (var i = 0; i < store.length; i++) {
                ////    for (var j = 0; j < selection.length; j++){
                ////        if (store[i] == selection[j]) {
                ////            T31Grid.getSelectionModel().select(i, true);
                ////        }
                ////    }
                ////}
                //
                //var selection = T31Grid.getSelection();
                //if (selection.length == 0) {
                //    Ext.getCmp('transferT31').disable();
                //}

            },
            itemclick: function (self, record, item, index, e, eOpts) {
                    var store = T31Grid.getStore().data.items;
                    for (var i = 0; i < store.length; i++) {
                    if (store[i] == record) {
                        if (record.data.PUR_QTY == null || record.data.PUR_QTY == '') {
                            T31Grid.getSelectionModel().deselect(i, true);
                        }
                    }
                }
                    var selection = T31Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.getCmp('transferT31').disable();
                    }
            },
            viewready: function (grid) {
                var view = grid.view;

                // record the current cellIndex
                grid.mon(view, {
                    uievent: function (type, view, cell, recordIndex, cellIndex, e) {
                        grid.cellIndex = cellIndex;
                        grid.recordIndex = recordIndex;
                    }
                });

                grid.tip = Ext.create('Ext.tip.ToolTip', {
                    target: view.el,
                    delegate: '.x-grid-cell',
                    trackMouse: true,
                    renderTo: Ext.getBody(),
                    listeners: {
                        beforeshow: function updateTipBody(tip) {
                            var is_valid = grid.getStore().getAt(grid.recordIndex).get('IS_VALID');
                            if (is_valid == 'Y') {
                                return false;
                            } else {
                                tip.update('單位劑量無法轉換，若需訂購請先前往維護');
                            }
                        }
                    }
                });

            }
        }
    });

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    var detailInsertWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items:[T31Grid],      
        modal: true,
        width: windowWidth,
        height:windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        y: 0,
        buttons: [{
            text: '關閉',
            handler: function () {
                detailInsertWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                Ext.getCmp('transferT31').disable();
                detailInsertWindow.setY(0);
            }
        }
    });
    detailInsertWindow.hide();
    //#endregion

    //#region orderWindow
    var T41Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
        },
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
         items: [{
            xtype: 'panel',
            id: 'PanelP41',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '訂單編號',
                    name: 'P41P0',
                    id: 'P41P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13,          // 可輸入最大長度為100
                    padding: '0 4 0 4',
                    width: 285
                },
                //{
                //    xtype: 'radiogroup',
                //    name: 'P41P1',
                //    fieldLabel: '藥品種類',
                //    items: [
                //        { boxLabel: '科學中藥', name: 'P41P1', inputValue: 'A', width: 100, checked: true },
                //        { boxLabel: '飲片', name: 'P41P1', inputValue: 'B', width: 100 }
                //    ]
                //},
                {
                    xtype: 'combo',
                    store: tcTypeStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '藥品種類',
                    name: 'P41P1',
                    id: 'P41P1',
                    labelWidth: mLabelWidth,
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 180,
                    value: ''

                },
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP42',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '訂購日期',
                    name: 'P41P2',
                    id: 'P41P2',
                    labelWidth: mLabelWidth,
                    width: 180,
                    allowBlank: true,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P41P3',
                    id: 'P41P3',
                    labelWidth: 8,
                    labelSeparator: '',
                    width: 98,
                    allowBlank: true,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    style: 'margin:0px 5px 0px 35px;',
                    handler: function () {
                        T41Load();
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    style: 'margin:0px 5px;',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('');
                    }
                }

            ]
        }]
    });
    var T41Tool = Ext.create('Ext.PagingToolbar', {
        store: T41Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '取消訂單',
                id: 'cancelOrder',
                disabled:true,
                name: 'cancelOrder',
                handler: function () {
                    var selection = T41Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.MessageBox.alert('提示', '請選擇要取消訂購的資料');
                        return;
                    }

                    Ext.MessageBox.confirm('提示', '是否確定取消訂購? ', function (btn, text) {
                        if (btn === 'yes') {
                            var pur_nos = '';
                            for (var i = 0; i < selection.length; i++) {
                                if (pur_nos != '') {
                                    pur_nos += ','
                                }
                                pur_nos += selection[i].data.PUR_NO;
                            }

                            cancelOrder(pur_nos);
                        }
                    });
                }
            }
        ]
    });
    var T41Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T41Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        dockedItems: [
        //T41Query,
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T41Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T41Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "訂單編號",
                dataIndex: 'PUR_NO',
                width: 150
            },
            {
                text: "訂購日期",
                dataIndex: 'PUR_DATE',
                width: 100
            },
            {
                text: "藥品種類",
                dataIndex: 'TC_TYPE_NAME',
                width: 100
            },
            {
                text: "訂購人",
                dataIndex: 'PUR_UNM_NAME',
                width: 100
            },
            {
                text: "訂購單備註",
                dataIndex: 'PUR_NOTE',
                width: 130,
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T42Load(record.data.PUR_NO);
            },
            selectionchange: function (model, records) {
                Ext.getCmp('cancelOrder').disable();
                
                if (records.length > 0) {
                    Ext.getCmp('cancelOrder').enable();
                } else {
                    T42Store.removeAll();
                }
            }
        }
    });
    var T42Tool = Ext.create('Ext.PagingToolbar', {
        store: T42Store,
        border: false,
        plain: true,
        displayInfo: true
    });
    var T42Grid = Ext.create('Ext.grid.Panel', {
        store: T42Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T42Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "電腦編號",
                dataIndex: 'MMCODE',
                width: 80,
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                width: 120
            },
            {
                text: "藥商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 120,
            },
            {
                text: "訂購量",
                dataIndex: 'PUR_QTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "庫存天數",
                dataIndex: 'INV_DAY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單位劑量",
                dataIndex: 'PUR_UNIT',
                width: 90
            },
            {
                text: "進貨單價",
                dataIndex: 'IN_PURPRICE',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "金額小計",
                dataIndex: 'PUR_AMOUNT',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    function cancelOrder(pur_nos) {
        myMask.show();
        Ext.Ajax.request({
            url: '/api/GA0002/CancelOrder',
            method: reqVal_p,
            params: {
                pur_nos: pur_nos
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                myMask.hide();
                if (data.success) {
                    msglabel('取消訂單完成');
                    T41Load();
                    T1Load();
                }
            },
            failure: function (response, options) {
                myMask.hide();
                Ext.MessageBox.alert('提示', '發生例外錯誤');
            }
        });
    }

    var orderWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items:[{
            xtype: 'container',
            layout: 'fit',
            items: [
                
                {
                    xtype: 'panel',
                    itemId: 't41Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    border: false,
                    split: true,
                    height: '50%',
                    items: [T41Grid]
                },
                {
                    xtype: 'panel',
                    autoScroll: true,
                    itemId: 't42Grid',
                    region: 'south',
                    layout: 'fit',
                    border: false,
                    height: '50%',
                    split: true,
                    items: [T42Grid]
                }
            ],
        }],
        modal: true,
        width: windowWidth,
        height: windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        y: 0,
        title: "取消訂單",
        buttons: [{
            text: '關閉',
            handler: function () {
                orderWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                orderWindow.setY(0);
            }
        }
    });
    orderWindow.hide();
    //#endregion


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
                        autoHeight: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        autoHeight: true,
                        split: true,
                        items: [T2Grid]
                    },
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                //items: [T1Query, T1Form]
                items: [T1Form]
            }
        ]
    });
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中' });
    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        T31Grid.setHeight(windowHeight - 75);
        T31Grid.setWidth(windowWidth);
        detailInsertWindow.setHeight(windowHeight);
        detailInsertWindow.setWidth(windowWidth);

        //T41Grid.setHeight((windowHeight - 100) / 2);
        //T42Grid.setHeight((windowHeight - 100) / 2);
        T41Grid.setHeight((windowHeight) / 2);
        T42Grid.setHeight((windowHeight) / 2);
        T41Grid.setWidth(windowWidth);
        T42Grid.setWidth(windowWidth);
        orderWindow.setHeight(windowHeight);
        orderWindow.setWidth(windowWidth);
    });
});




