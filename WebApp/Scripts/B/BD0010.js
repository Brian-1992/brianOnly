Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

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


Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Set = '';
    var T1LastRec = null, T2LastRec = null;
    var T1Name = '藥品訂單維護';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var reportUrl = '/Report/B/BD0010.aspx';

    var viewModel = Ext.create('WEBAPP.store.BD0010VM');

    var WH_NO = '';
    var YYYYMMDD = '';
    var YYYYMMDD_E = '';
    var WH_NAME = '';
    var PO_STATUS = '';
    var Agen_No = '';

    var T1Store = viewModel.getStore('MM_PO_M');
    function T1Load() {

        T1Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('WH_NO').getValue());
        T1Store.getProxy().setExtraParam("YYYYMMDD", T1Query.getForm().findField('P1').rawValue);
        T1Store.getProxy().setExtraParam("YYYYMMDD_E", T1Query.getForm().findField('P4').rawValue);
        T1Store.getProxy().setExtraParam("PO_STATUS", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("Agen_No", T1Query.getForm().findField('P3').getValue());

        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('MM_PO_D');
    function T2Load() {


        if (T1LastRec != null && T1LastRec.data["PO_NO"] !== '') {

            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["PO_NO"]);
            T2Tool.moveFirst();

        }
    }

    //建立查詢庫別的下拉式選單
    function SetWH_NO() {
        Ext.Ajax.request({
            url: GetWH_NO,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            WH_NO_Store.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                            first_wh_no = wh_nos[0].VALUE;
                            T1Query.getForm().findField("WH_NO").setValue(wh_nos[0].VALUE);
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    var AgenComboGet = '../../../api/BD0010/GetAgenCombo';
    var GetWH_NO = '../../../api/BD0010/GetWH_NO';

    // 物品類別清單
    var AgenQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //新增庫別的store
    var WH_NO_Store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: AgenComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var agen_nos = data.etts;
                    if (agen_nos.length > 0) {
                        for (var i = 0; i < agen_nos.length; i++) {
                            AgenQueryStore.add({ VALUE: agen_nos[i].VALUE, TEXT: agen_nos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
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
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '庫別',
                            name: 'WH_NO',
                            id: 'WH_NO',
                            labelWidth: 50,
                            width: 200,
                            store: WH_NO_Store,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            autoSelect: true,
                            anyMatch: true,
                            fieldCls: 'required',
                            allowBlank: false, // 欄位是否為必填
                            blankText: "請輸入庫別",
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '採購日期',
                            name: 'P1',
                            enforceMaxLength: true,
                            maxLength: 7,
                            labelWidth: 80,
                            width: 160,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls: 'required',
                            regexText: '請選擇日期'
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '迄',
                            name: 'P4',
                            enforceMaxLength: true,
                            maxLength: 7,
                            labelWidth: 20,
                            width: 100,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls: 'required',
                            regexText: '請選擇日期',
                        }, {
                            xtype: 'combo',
                            fieldLabel: '訂單狀態',
                            name: 'P2',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 155,
                            padding: '0 4 0 4',
                            store: [
                                { VALUE: '', TEXT: '全部' },
                                { VALUE: '0', TEXT: '未取消訂單' },
                                { VALUE: 'D', TEXT: 'D 取消訂單' }
                            ],
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            value: '',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '廠商代碼',
                            name: 'P3',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            store: AgenQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');

                                if (T1Query.getForm().isValid()) {
                                    WH_NO = T1Query.getForm().findField('WH_NO').getValue();
                                    WH_NAME = T1Query.getForm().findField('WH_NO').rawValue;
                                    YYYYMMDD = T1Query.getForm().findField('P1').rawValue;
                                    YYYYMMDD_E = T1Query.getForm().findField('P4').rawValue;
                                    PO_STATUS = T1Query.getForm().findField('P2').getValue();
                                    Agen_No = T1Query.getForm().findField('P3').getValue();
                                    T1Load();
                                    T2Store.removeAll();
                                } else {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                                }


                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }
            ]
        }]
    });
    function getDefaultValue(isEndDate) {
        var yyyy = new Date().getFullYear() - 1911;
        var m = new Date().getMonth() + 1;
        var d = 0;
        if (isEndDate) {
            d = new Date(yyyy, m, 0).getDate();
        } else {
            d = 1;
        }
        var mm = m > 10 ? m.toString() : "0" + m.toString();
        var dd = d > 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;

    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {
                    setFormVisible(true, false);
                    T1Set = '/api/BD0010/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '寄送MAIL',
                id: 'btnSendMail',
                name: 'btnSendMail',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let po_no = '';
                        //selection.map(item => {
                        //    po_no += item.get('PO_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            po_no += item.get('PO_NO') + ',';
                        })
                        Ext.MessageBox.confirm('寄送MAIL', '已傳MAIL資料不會再寄(要點[補發MAIL]), 要進行採購訂單MAIL發送作業?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0010/CheckPoAmt',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success == false) {
                                            T3Store.loadData(data.etts);
                                            over100KWindow.show();
                                        } else {
                                            sendEmail(po_no)
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
            },
            {
                text: '申購報表',
                id: 'btnReport',
                name: 'btnReport',
                handler: function () {
                    showReport();
                }
            },
            {
                text: '補發MAIL',
                id: 'btnReSendMail',
                name: 'btnReSendMail',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let po_no = '';
                        //selection.map(item => {
                        //    po_no += item.get('PO_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            po_no += item.get('PO_NO') + ',';
                        })

                        Ext.MessageBox.confirm('寄送MAIL', '要進行採購訂單<span style=color:red>補發</span>MAIL發送作業?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0010/ReSendEmail',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除申請單號<br>' + name + '成功');
                                            msglabel('訊息區:資料更新成功');
                                            //T2Store.removeAll();

                                            T1Grid.getSelectionModel().deselectAll();
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
                text: '整份作廢',
                id: 'btnObsolete',
                name: 'btnObsolete',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let po_no = '';
                        //selection.map(item => {
                        //    po_no += item.get('PO_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            po_no += item.get('PO_NO') + ',';
                        })

                        Ext.MessageBox.confirm('整份作廢', '要進行整份作廢作業?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0010/MasterObsolete',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料更新成功');
                                            //T2Store.removeAll();

                                            T1Grid.getSelectionModel().deselectAll();
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
        ]
    });
    function sendEmail(po_nos) {
        Ext.Ajax.request({
            url: '/api/BD0010/SendEmail',
            method: reqVal_p,
            params: {
                PO_NO: po_nos
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    //Ext.MessageBox.alert('訊息', '刪除申請單號<br>' + name + '成功');
                    msglabel('訊息區:資料更新成功');
                    //T2Store.removeAll();

                    T1Grid.getSelectionModel().deselectAll();
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
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox

            selectable: function (record) {
                return (record.get('PO_STATUS') != '開單' && record.get('PO_STATUS') != '已傳MAIL');
            },
        },
        ),
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "採購日期",
                dataIndex: 'PO_DATE',
                width: 80
            },
            {
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 90
            },
            {
                text: "庫別代碼",
                dataIndex: 'WH_NO',
                width: 70
            },
            {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 190
            },
            {
                text: "合約碼",
                dataIndex: 'CONTRACNO',
                width: 50
            },
            {
                text: "採購筆數",
                dataIndex: 'CNT',
                width: 70,
                align: 'right',
                style: 'text-align:left',

            },
            {
                text: "訂單狀態",
                dataIndex: 'PO_STATUS',
                width: 80
            },
            {
                text: "收信確認日期",
                dataIndex: 'REPLY_DT',
                width: 100
            },
            {
                text: "送貨資料回覆",
                dataIndex: 'REPLY_DELI',
                width: 80
            },
            {
                text: "MAIL備註",
                dataIndex: 'MEMO',
                width: 350
            },
            {
                text: "MAIL特殊備註",
                dataIndex: 'SMEMO',
                width: 200
            },
            {
                header: "",
                flex: 1
            }
        ],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Grid.down('#btnUpdate').setDisabled(true);
                    T1Grid.down('#btnObsolete').setDisabled(true);
                    T1Grid.down('#btnReSendMail').setDisabled(true);
                    T1Grid.down('#btnReport').setDisabled(true);

                    if (T1Store.getCount() > 0) {
                        T1Grid.down('#btnReport').setDisabled(false);
                        T1Grid.down('#btnSendMail').setDisabled(false);
                    }

                }
            }
        },
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {


                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T1LastRec = record;

                if (record.data.PO_STATUS == '開單' || record.data.PO_STATUS == '已傳MAIL') {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    Ext.getCmp('btnSendMail').setDisabled(false);
                    Ext.getCmp('btnObsolete').setDisabled(false);
                    Ext.getCmp('btnReSendMail').setDisabled(false);
                }


                setFormT1a();

                setFormVisible(true, false);

                //Tabs.setActiveTab('Form');

                T2Load();
            }
        }


    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                disabled: true,
                handler: function () {
                    setFormVisible(true, false);
                    T1Set = '/api/BD0010/DetailUpdate';
                    setFormT2('U', '修改');
                }
            },
            {
                text: '單筆作廢',
                id: 'btnObsolete2',
                name: 'btnObsolete2',
                disabled: true,
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let po_no = T1LastRec.data["PO_NO"];
                        let mmcode = '';
                        //selection.map(item => {
                        //    mmcode += item.get('MMCODE') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            mmcode += item.get('MMCODE') + ',';
                        })

                        Ext.MessageBox.confirm('單筆作廢', '要進行單筆作廢作業?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0010/DetailObsolete',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no,
                                        MMCODE: mmcode
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料更新成功');

                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();

                                            if (data.msg != '') {
                                                Ext.MessageBox.alert('提示', data.msg);
                                            }
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
            }]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "藥品院內碼",
                dataIndex: 'MMCODE',
                width: 100,
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                xtype: 'hidden',
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "原製造商",
                dataIndex: 'E_MANUFACT',
                width: 100,
                style: 'text-align:left'
            },

            {
                text: "單位",
                dataIndex: 'M_PURUN',
                width: 70,
                style: 'text-align:left'
            },
            {
                text: "單價",
                dataIndex: 'PO_PRICE',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "數量",
                dataIndex: 'PO_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "金額",
                dataIndex: 'PO_AMT',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "狀態",
                dataIndex: 'STATUS',
                width: 70,
                style: 'text-align:left'
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 150,
                style: 'text-align:left'
            },
            {
                text: "合約碼",
                dataIndex: 'CONTRACNO',
                width: 70,
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }
        ],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T2Grid.down('#btnUpdate2').setDisabled(true);
                    T2Grid.down('#btnObsolete2').setDisabled(true);
                }
            }
        },
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {


                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T2LastRec = record;

                if (T2Grid.getSelection().length > 0) {
                    Ext.getCmp('btnUpdate2').setDisabled(false);
                    T2Grid.down('#btnObsolete2').setDisabled(false);
                }


                setFormT2a();

                setFormVisible(false, true);

                Ext.getCmp('eastform').expand();

            }
        }
    });

    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        f.findField("PO_NO").setValue(T1LastRec.data["PO_NO"]);
        f.findField("PO_DATE").setValue(T1LastRec.data["PO_DATE"]);
        f.findField("CONTRACNO").setValue(T1LastRec.data["CONTRACNO"]);
        f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
        f.findField("PO_STATUS").setValue(T1LastRec.data["PO_STATUS"]);
        f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);
        f.findField("SMEMO").setValue(T1LastRec.data["SMEMO"]);

        f.findField('x').setValue(x);
        f.findField("MEMO").setReadOnly(false);
        f.findField("SMEMO").setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        var u = f.findField('MEMO');
        u.focus();
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            f.findField("PO_NO").setValue(T1LastRec.data["PO_NO"]);
            f.findField("PO_DATE").setValue(T1LastRec.data["PO_DATE"]);
            f.findField("CONTRACNO").setValue(T1LastRec.data["CONTRACNO"]);
            f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
            f.findField("PO_STATUS").setValue(T1LastRec.data["PO_STATUS"]);
            f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);
            f.findField("SMEMO").setValue(T1LastRec.data["SMEMO"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: { type: 'table', columns: 1 },
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
                name: 'PO_NO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '採購日期',
                name: 'PO_DATE',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '合約碼',
                name: 'CONTRACNO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '廠商',
                name: 'AGEN_NO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '訂單狀態',
                name: 'PO_STATUS',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textareafield',
                fieldLabel: 'MAIL備註',
                name: 'MEMO',
                enforceMaxLength: true,
                maxLength: 4000,
                readOnly: true,
                height: 200,
                width: "100%"
            },
            {
                xtype: 'textareafield',
                fieldLabel: 'MAIL特殊備註(MAIL紅字顯示)',
                name: 'SMEMO',
                enforceMaxLength: true,
                maxLength: 2000,
                readOnly: true,
                height: 150,
                width: "100%"
            }
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

                    T1Load();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            //Ext.Msg.alert('訊息', '新增成功');
                            break;
                        case "U":

                            msglabel('訊息區:資料更新成功');
                            //Ext.Msg.alert('訊息', '更新成功');
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
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield" || fc.xtype === "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

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
                    T2Load();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            //Ext.Msg.alert('訊息', '新增成功');
                            break;
                        case "U":
                            msglabel('訊息區:資料更新成功');
                            //Ext.Msg.alert('訊息', '更新成功');
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
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield" || fc.xtype === "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
        T2Form.down('#cancel').hide();
        T2Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {

        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();

        setFormVisible(false, true);
        var f = T2Form.getForm();

        u = f.findField('PO_QTY');


        f.findField('x').setValue(x);
        f.findField('PO_QTY').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);
        T2Form.down('#cancel').setVisible(true);

        T2Form.down('#submit').setVisible(true);
        u.focus();
    }
    function meDocdSeqPromise(docno) {
        var deferred = new Ext.Deferred();

        Ext.Ajax.request({
            url: GetMeDocdMaxSeq,
            method: reqVal_p,
            params: {
                DOCNO: docno
            },
            success: function (response) {
                deferred.resolve(response.responseText);
            },

            failure: function (response) {
                deferred.reject(response.status);
            }
        });

        return deferred.promise; //will return the underlying promise of deferred

    }

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();

            f.findField("CONTRACNO").setValue(T1LastRec.data["CONTRACNO"]);
            f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
            f.findField("PO_NO").setValue(T1LastRec.data["PO_NO"]);

            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('E_MANUFACT').setValue(T2LastRec.data["E_MANUFACT"]);
            f.findField('M_PURUN').setValue(T2LastRec.data["M_PURUN"]);
            f.findField('PO_PRICE').setValue(T2LastRec.data["PO_PRICE"]);
            f.findField('PO_QTY').setValue(T2LastRec.data["PO_QTY"]);
            f.findField('PO_AMT').setValue(T2LastRec.data["PO_AMT"]);
            f.findField('STATUS').setValue(T2LastRec.data["STATUS"]);
            f.findField('MEMO').setValue(T2LastRec.data["MEMO"]);
        }

    }

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        defaultType: 'textfield',
        items: [
            {
                xtype: 'container',
                items: [
                    {
                        fieldLabel: 'Update',
                        name: 'x',
                        xtype: 'hidden'
                    },
                    {
                        xtype: 'hidden',
                        fieldLabel: '訂單編號',
                        name: 'PO_NO',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '合約碼',
                        name: 'CONTRACNO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商',
                        name: 'AGEN_NO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '原製造商',
                        name: 'E_MANUFACT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單位',
                        name: 'M_PURUN',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單價',
                        name: 'PO_PRICE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '數量',
                        name: 'PO_QTY',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        minValue: 1,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '金額',
                        name: 'PO_AMT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '狀態',
                        name: 'STATUS',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'

                    },
                    {
                        xtype: 'textareafield',
                        fieldLabel: '備註',
                        name: 'MEMO',
                        enforceMaxLength: true,
                        maxLength: 2000,
                        readOnly: true,
                        height: 150,
                        labelAlign: 'right',

                        width: "100%"
                    }
                ]
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
                                T2Submit();

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
    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?WH_NO=' + WH_NO + '&YYYYMMDD=' + YYYYMMDD + '&YYYYMMDD_E=' + YYYYMMDD_E + '&PO_STATUS=' + PO_STATUS + '&Agen_No=' + Agen_No + '&RptFrom=BD0010" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }

    //#region 2021-03-23 零購品採購當次超過十萬元，於藥庫實際發送訂單前跳出提示訊息並列出所有超過十萬元的院內碼但不鎖控
    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },     
            { name: 'PO_QTY', type: 'string' },    
            { name: 'PO_AMT', type: 'string' }
        ]
    });
    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        sorters: [{ property: 'PO_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
    });
    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 330,
        columns: [
            {
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                width: 120
            },
            {
                text: "數量",
                dataIndex: 'PO_QTY',
                width: 80
            },
            {
                text: "單價",
                dataIndex: 'PO_PRICE',
                width: 80
            },
            {
                text: "金額",
                dataIndex: 'PO_AMT',
                width: 100
            },
            {
                header: "",
                flex: 1
            }

        ],
    });
    var over100KWindow = Ext.create('Ext.window.Window', {
        id: 'over100KWindow',
        renderTo: Ext.getBody(),
        items: [T3Grid],
        modal: true,
        width: "400px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "零購超過十萬元清單",
        buttons: [
            {
                text: '確定並匯出',
                handler: function () {
                    console.log('確定並匯出');

                    // 下載excel
                    
                    var selection = T1Grid.getSelection();
                    let po_no = '';
                    $.map(selection, function (item, key) {
                        po_no += item.get('PO_NO') + ',';
                    })
                    var p = new Array();
                    p.push({ name: 'PO_NO', value: po_no });
                    PostForm('../../../api/BD0010/Over100KExcel', p);

                    sendEmail(po_no)

                    over100KWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                over100KWindow.center();
                over100KWindow.setWidth(400);
            }
        }
    });
    over100KWindow.hide();
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
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form, T2Form]
            }
        ]
    });
    SetWH_NO();
    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnSendMail').setDisabled(true);
    T1Query.getForm().findField('P1').setValue(new Date());
    T1Query.getForm().findField('P4').setValue(new Date());
});