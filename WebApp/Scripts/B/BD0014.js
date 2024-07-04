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
            hdSelectStatus = storeCount === selectedCount;
        }

        if (views && views.length) {
            me.column.setHeaderStatus(hdSelectStatus);
        }
    },
});


Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    var menuLink = Ext.getUrlParam('menuLink');
    var hospCode = '';
    var T1Set = '';
    var T1LastRec = null, T2LastRec = null;
    var T1Name = '訂單維護';

    var WH_NO = '';
    var YYYYMMDD = '';
    var YYYYMMDD_E = '';
    var WH_NAME = '';
    var PO_STATUS = '';
    var Agen_No = '';
    var MMCODE = '';
    var reportUrl = '/Report/B/BD0014.aspx';

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'PO_TIME', type: 'string' },
            { name: 'WH_NO', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'PO_STATUS', type: 'string' },
            { name: 'CNT', type: 'string' },
            { name: 'MMCODE_OVER_150K', type: 'string' },
            { name: 'PR_USER', type: 'string' },
            { name: 'CREATE_TIME', type: 'date' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'date' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'EASYNAME', type: 'string' },
            { name: 'EMAIL', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0014/MasterAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    wh_no: T1Query.getForm().findField('WH_NO').getValue(), // wh_no 庫房代碼
                    start_date: T1Query.getForm().findField('P1').rawValue, // start_date
                    end_date: T1Query.getForm().findField('P4').rawValue, // end_date
                    mmcode: T1Query.getForm().findField('P5').getValue(), // mmcode 院內碼 08021274
                    agen_no: T1Query.getForm().findField('P3').getValue(), // agen_no 廠商代碼
                    po_status: T1Query.getForm().findField('P2').getValue(), // P2, 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]
                    endl: ''
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {

        T1Tool.moveFirst();
        T1Grid.down('#btnUpdate').setDisabled(true); // 修改
        T1Grid.down('#btnSendMail').setDisabled(true); // 寄送MAIL
        T1Grid.down('#btnReport').setDisabled(true); // 訂單列印
        T1Grid.down('#btnObsolete').setDisabled(true); // 補發mail
        T1Grid.down('#btnReSendMail').setDisabled(true); // 整份作廢
        T1Grid.down('#btnExportDetailByExcel').setDisabled(true);  // ★匯出明細
        T1Grid.down('#btnExportDetailByZip').setDisabled(true);  // ★匯出壓縮檔
        T1Grid.down('#btnFax').setDisabled(true);
    }

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'PO_QTY', type: 'string' },
            { name: 'PO_PROCE', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'DELI_QTY', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'M_NHKEY', type: 'string' },
            { name: 'E_SOURCECODE', type: 'string' },
            { name: 'UNITRATE', type: 'string' },
            { name: 'CASENO', type: 'string' },
            { name: 'STATUS', type: 'string' },
            { name: 'M_CONTID', type: 'date' },
            { name: 'WH_NO', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'CREATE_TIME', type: 'date' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'date' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'MAST_UNITRATE', type: 'string' },
            { name: 'PARTIALDL_DT', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0014/DetailAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1LastRec.data.PO_NO
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {

        T2Tool.moveFirst();
    }

    //建立查詢庫別的下拉式選單
    function SetWH_NO() {
        Ext.Ajax.request({
            url: '/api/BD0014/GetWH_NO',
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
    var AgenComboGet = '../../../api/BD0014/GetAgenCombo';
    var MmcodeComboGet = '../../../api/BD0014/GetMmcodeCombo';
    var MatClassComboGet = '../../../api/BD0014/GetMatClassCombo'; // 物料類別
    var MatClassTextArea = '../../../api/BD0014/GetMatClassTextArea'; // 物料類別的信件內容
    var CreateEmailAttFile = '../../../api/BD0014/GetCreateEmailAttFile';
    // 物料類別清單
    var MatClassQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 物品類別清單
    var AgenQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 院內碼(combo，smart query，若物料分類有選需帶入為條件) 
    var MmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //新增庫別的store
    var WH_NO_Store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //新增庫別的store
    var status_Store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //新增備註的store
    var memo_Store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: AgenComboGet, // '../../../api/BD0014/GetAgenCombo';
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
        //院內碼(combo，smart query，若物料分類有選需帶入為條件) , select mmcode, mmname_c, mmname_e from MI_MAST where mat_class in ('01', '02', '03', '04', '05', '06', '07', '08') and nvl(cancel_id, 'N') = 'N'
        Ext.Ajax.request({
            url: MmcodeComboGet, // '../../../api/BD0014/GetMmcodeCombo';
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var agen_nos = data.etts;
                    if (agen_nos.length > 0) {
                        for (var i = 0; i < agen_nos.length; i++) {
                            MmcodeQueryStore.add({ VALUE: agen_nos[i].VALUE, TEXT: agen_nos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        //物料類別
        Ext.Ajax.request({
            url: MatClassComboGet, // '../../../api/BD0014/GetMatClassCombo';
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var nos = data.etts;
                    if (nos.length > 0) {
                        for (var i = 0; i < nos.length; i++) {
                            MatClassQueryStore.add({ VALUE: nos[i].VALUE, TEXT: nos[i].TEXT });
                        }
                        var mat_class_combo = Ext.getCmp('MAT_CLASS'); // 如果已經有 ID，使用此方法來取得物件
                        // 或者使用變數方式取得 Combo Box
                        // var combo = new Ext.form.ComboBox({ ... });
                        // 設定預設選取第一筆資料
                        var mat_class_combo_val = mat_class_combo.getStore().getAt(0).get("VALUE");
                        mat_class_combo.setValue(mat_class_combo_val);
                        mat_class_combo.fireEvent('select', mat_class_combo, mat_class_combo.getStore().getAt(0), 0);
                    }
                }
            },
            failure: function (response, options) {
                ;
            }
        });
        //狀態
        Ext.Ajax.request({
            url: '/api/BD0014/GetStatusCombo', // '../../../api/BD0014/GetMmcodeCombo';
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var status = data.etts;
                    if (status.length > 0) {
                        for (var i = 0; i < status.length; i++) {
                            status_Store.add({ VALUE: status[i].VALUE, TEXT: status[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        //備註
        Ext.Ajax.request({
            url: '/api/BD0014/GetMemoCombo', // '../../../api/BD0014/GetMmcodeCombo';
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var memo = data.etts;
                    if (memo.length > 0) {
                        for (var i = 0; i < memo.length; i++) {
                            memo_Store.add({ VALUE: memo[i].VALUE });
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
            layout: 'vbox',
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
                            labelWidth: 60,
                            width: 200,
                            padding: '0 4 0 4',
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
                            labelWidth: 70,
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
                            labelWidth: 10,
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
                            store: status_Store,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            value: '',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '院內碼',
                            name: 'P5',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 350,
                            padding: '0 4 0 4',
                            store: MmcodeQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '廠商代碼',
                            name: 'P3',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 281,
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

                                //console.log("wh_no=" + T1Query.getForm().findField('WH_NO').getValue()); // 560000
                                //console.log("wh_no=" + T1Query.getForm().findField('WH_NO').rawValue); // wh_no 560000 中央庫房衛材庫
                                //console.log("P1=" + T1Query.getForm().findField('P1').getValue()); // start_date P1=Sun Jun 18 2023 00:00:00 GMT+0800 (台北標準時間)
                                //console.log("P1=" + T1Query.getForm().findField('P1').rawValue); // 1120618
                                //console.log("P4=" + T1Query.getForm().findField('P4').getValue()); // end_date
                                //console.log("P4=" + T1Query.getForm().findField('P4').rawValue);
                                //console.log("P5=" + T1Query.getForm().findField('P5').getValue()); // mmcode [null|005ZYP04]
                                //console.log("P5=" + T1Query.getForm().findField('P5').rawValue); // mmcode [無|'津普速 口溶錠5毫克']
                                //console.log("P3=" + T1Query.getForm().findField('P3').getValue()); // agen_no [null|'001']
                                //console.log("P3=" + T1Query.getForm().findField('P3').rawValue); // agen_no [無|'112061900005']
                                //console.log("P2=" + T1Query.getForm().findField('P2').getValue()); // 訂單狀態 [無]
                                //console.log("P2=" + T1Query.getForm().findField('P2').rawValue); // 訂單狀態 ['全部']


                                if (T1Query.getForm().isValid()) {
                                    WH_NO = T1Query.getForm().findField('WH_NO').getValue();
                                    WH_NAME = T1Query.getForm().findField('WH_NO').rawValue;
                                    YYYYMMDD = T1Query.getForm().findField('P1').rawValue;
                                    YYYYMMDD_E = T1Query.getForm().findField('P4').rawValue;
                                    PO_STATUS = T1Query.getForm().findField('P2').getValue(); // 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]
                                    Agen_No = T1Query.getForm().findField('P3').getValue();
                                    MMCODE = T1Query.getForm().findField('P5').getValue();
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
                        },
                        {
                            xtype: 'button',
                            text: '訂單訊息修改',
                            id: 'btnMessageUpdate',
                            margin: '0 0 0 8',
                            handler: function () {
                                editPOWindow.show();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '一鍵寄送',
                            id: 'btnSendMailAll',
                            name: 'btnSendMailAll',
                            handler: function () {
                                Ext.MessageBox.confirm('一鍵寄送', '確定將所有開單訂單進行採購MAIL發送作業?', function (btn, text) {
                                    if (btn === 'yes') {
                                        // 先檢查訂單是否有金額超出定額或廠商沒有EMAIL,有這些資料時顯示確認視窗
                                        Ext.Ajax.request({
                                            url: '/api/BD0014/ChkSendMailAll',
                                            method: reqVal_p,
                                            params: {
                                                wh_no: T1Query.getForm().findField('WH_NO').getValue(), // wh_no
                                                start_date: T1Query.getForm().findField('P1').rawValue, // start_date
                                                end_date: T1Query.getForm().findField('P4').rawValue // end_date
                                            },
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                if (data.success == false) {
                                                    var textareafield = Ext.getCmp('SENDALL_CHK_RESULT');
                                                    textareafield.setValue(data.msg);
                                                    sendAllChkWindow.show();
                                                    if (data.afrs > 0) {
                                                        var GetExcel = '../../../api/BD0014/ExportNoEmailSupplier';
                                                        var p = new Array();
                                                        p.push({ name: 'wh_no', value: T1Query.getForm().findField('WH_NO').getValue() });
                                                        p.push({ name: 'start_date', value: T1Query.getForm().findField('P1').rawValue });
                                                        p.push({ name: 'end_date', value: T1Query.getForm().findField('P4').rawValue });
                                                        PostForm(GetExcel, p);
                                                    }
                                                } else {
                                                    sendEmailAll(); // 若沒有需確認的資料則直接進行寄送作業
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
                    T1Set = '/api/BD0014/MasterUpdate';
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
                        // A.逐筆檢查所選單號之廠商是否皆有EMAIL，若無EMAIL則跳出訊息「{廠商代碼}無EMAIL資訊 ，請維護後再寄信」。
                        //   select 1 from MM_PO_M a, PH_VENDER b where a.agen_no = b.agen_no and nvl(b.email, 'N') <> 'N'
                        let agen_nos = '';
                        //selection.map(item => {
                        //    po_no += item.get('PO_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            agen_nos += item.get('AGEN_NO') + ',';
                        })
                        Ext.Ajax.request({
                            url: '/api/BD0014/CheckAll_PH_VENDER_had_mail', // CheckPoAmt
                            method: reqVal_p,
                            params: {
                                AGEN_NOS: agen_nos
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success == false) { // 都沒有mail，提示要維護mail
                                    // data.etts
                                    Ext.MessageBox.alert('提示', data.msg);
                                } else {
                                    //--
                                    let po_no = '';
                                    //selection.map(item => {
                                    //    po_no += item.get('PO_NO') + ',';
                                    //});
                                    $.map(selection, function (item, key) {
                                        po_no += item.get('PO_NO') + ',';
                                    })
                                    Ext.MessageBox.confirm('寄送MAIL', '已傳MAIL資料不會再寄(要點[補發MAIL]), 要進行採購訂單MAIL發送作業?', function (btn, text) {
                                        if (btn === 'yes') {
                                            ;
                                            //B.檢查是否有非合約年度累積超過十五萬項目，若有跳出視窗供確認(BD0014Controller.CheckPoAmt) ，
                                            //  確認時需下載EXCEL(檔名：非合約超過十五萬元清單_{ 年月日時分秒 }.xls)後才可修改主檔狀態。若按取消則關閉視窗並不進行後續作業。
                                            Ext.Ajax.request({
                                                url: '/api/BD0014/CheckPoAmt',
                                                method: reqVal_p,
                                                params: {
                                                    PO_NO: po_no
                                                },
                                                success: function (response) {
                                                    var data = Ext.decode(response.responseText);
                                                    ;
                                                    if (data.success == false) {
                                                        T3Store.loadData(data.etts);
                                                        over100KWindow.show();
                                                    } else {
                                                        sendEmail(po_no)
                                                    }
                                                },
                                                failure: function (response) {
                                                    ;
                                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                }
                                            });
                                        }
                                    });
                                    //-- 
                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        }); // end of /api/BD0014/CheckAll_PH_VENDER_had_mail
                    }
                }
            },
            {
                text: '訂單列印',
                id: 'btnReport',
                name: 'btnReport',
                handler: function () {
                    var selectedRecords = T1Grid.getSelectionModel().getSelection();
                    var PO_NOs = [];
                    selectedRecords.forEach(function (record) {
                        PO_NOs.push(record.get('PO_NO'));
                    });
                    Ext.Ajax.request({
                        url: '/api/BD0014/CheckNotD',
                        method: reqVal_p,
                        params: {
                            PO_NOs: PO_NOs
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success == false) {
                                var po_noList = data.msg.split(',');
                                if (po_noList.length > 0) {
                                    var errorMsg = '';
                                    $.each(po_noList, function (index, item) {
                                        errorMsg += '<br/><span>' + item + '</span>';
                                    });
                                    Ext.MessageBox.alert("提示", "<span style=\"color: blue; font-weight: bold\">訂單所有品項皆已作廢，請重新確認</span>" + errorMsg);

                                }
                                return;
                            }

                            var PO_NOstr = '';
                            $.each(PO_NOs, function (index, item) {
                                PO_NOstr += item + ',';
                            });
                            // 若為松山,詢問是否要將本次列印的單子改為已傳真
                            if (hospCode == '807') {
                                Ext.Ajax.request({
                                    url: '/api/BD0014/ChkAgenCnt',
                                    method: reqVal_p,
                                    params: {
                                        po_no: PO_NOstr
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success == true) {
                                            var isMultiAgen = data.msg;
                                            var msgText = '';
                                            var PO_NOmsg = '';
                                            $.each(PO_NOs, function (index, item) {
                                                PO_NOmsg += item + '<br>';
                                            });
                                            if (isMultiAgen == 'Y') {
                                                msgText = '本次列印<span style=color:red>包含多個廠商</span>,是否要將這些訂單設定為已傳真?<br>' + PO_NOmsg;
                                            }
                                            else {
                                                msgText = '是否要將本次列印訂單設定為已傳真?<br>' + PO_NOmsg;
                                            }
                                            Ext.Msg.show({
                                                title: '列印訂單',
                                                message: msgText,
                                                buttons: Ext.Msg.YESNO,
                                                buttonText: {
                                                    yes: '是',
                                                    no: '否'
                                                },
                                                icon: Ext.Msg.QUESTION,
                                                fn: function (btn) {
                                                    if (btn === 'yes') {
                                                        Ext.Ajax.request({
                                                            url: '/api/BD0014/UpdateStatusOnFax',
                                                            method: reqVal_p,
                                                            params: {
                                                                po_no: PO_NOstr
                                                            },
                                                            success: function (response) {
                                                                var data = Ext.decode(response.responseText);
                                                                if (data.success == true) {
                                                                    showReport(PO_NOs);
                                                                }
                                                                else
                                                                    Ext.Msg.alert('錯誤', '更新單據狀態失敗');
                                                            },
                                                            failure: function (response, options) {
                                                                Ext.Msg.alert('提醒', '更新單據狀態時發生例外錯誤');
                                                            }
                                                        });
                                                    }
                                                    else {
                                                        showReport(PO_NOs);
                                                    }
                                                }
                                            });
                                        }
                                    },
                                    failure: function (response, options) {
                                    }
                                });
                            }
                            else
                                showReport(PO_NOs);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });

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
                                    url: '/api/BD0014/ReSendEmail',
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
                                    url: '/api/BD0014/MasterObsolete',
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
            {
                text: '匯出明細', // 勾選主檔多筆單號匯出，跳出視窗選擇匯出明細或是壓縮檔。匯出為XLSX格式，處理可參考AB0102Controller.Excel 
                id: 'btnExportDetailByExcel',
                name: 'btnExportDetailByExcel',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let po_no = '';
                        let easyname = "";
                        $.map(selection, function (item, key) {
                            po_no += item.get('PO_NO') + ',';
                            easyname = item.get('EASYNAME');
                        })
                        /*
                        console.log("wh_no=" + T1Query.getForm().findField('WH_NO').getValue()); // 560000
                        console.log("wh_no=" + T1Query.getForm().findField('WH_NO').rawValue); // wh_no 560000 中央庫房衛材庫
                        console.log("P1=" + T1Query.getForm().findField('P1').getValue()); // start_date P1=Sun Jun 18 2023 00:00:00 GMT+0800 (台北標準時間)
                        console.log("P1=" + T1Query.getForm().findField('P1').rawValue); // 1120618
                        console.log("P4=" + T1Query.getForm().findField('P4').getValue()); // end_date
                        console.log("P4=" + T1Query.getForm().findField('P4').rawValue);
                        console.log("P5=" + T1Query.getForm().findField('P5').getValue()); // mmcode [null|005ZYP04]
                        console.log("P5=" + T1Query.getForm().findField('P5').rawValue); // mmcode [無|'津普速 口溶錠5毫克']
                        console.log("P3=" + T1Query.getForm().findField('P3').getValue()); // agen_no [null|'001']
                        console.log("P3=" + T1Query.getForm().findField('P3').rawValue); // agen_no [無|'112061900005']
                        console.log("P2=" + T1Query.getForm().findField('P2').getValue()); // 訂單狀態 [無], PO_STATUS = T1Query.getForm().findField('P2').getValue(); // 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]
                        console.log("P2=" + T1Query.getForm().findField('P2').rawValue); // 訂單狀態 ['全部']
                    */
                        var param = new Array();
                        param.push({ name: 'wh_no', value: T1Query.getForm().findField('WH_NO').getValue() }); // wh_no 庫房代碼
                        param.push({ name: 'start_date', value: T1Query.getForm().findField('P1').rawValue }); // start_date
                        param.push({ name: 'end_date', value: T1Query.getForm().findField('P4').rawValue }); // end_date
                        param.push({ name: 'po_no', value: po_no }); // 訂單編號
                        param.push({ name: 'mmcode', value: T1Query.getForm().findField('P5').getValue() }); // mmcode 院內碼 08021274
                        param.push({ name: 'agen_no', value: T1Query.getForm().findField('P3').getValue() }); // agen_no 廠商代碼
                        param.push({ name: 'po_status', value: T1Query.getForm().findField('P2').getValue() }); // 訂單狀態 [無], PO_STATUS = T1Query.getForm().findField('P2').getValue(); // 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]
                        param.push({ name: 'easyname', value: easyname }); //廠商簡稱
                        PostForm('../../../api/BD0014/ExportDetailByExcel', param);
                    }
                }
            },
            {
                text: '匯出壓縮檔', // 勾選主檔多筆單號匯出，跳出視窗選擇匯出明細或是壓縮檔。匯出為XLSX格式，處理可參考AB0102Controller.Excel 
                id: 'btnExportDetailByZip',
                name: 'btnExportDetailByZip',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let po_no = '';
                        let email = '';
                        //selection.map(item => {
                        //    po_no += item.get('PO_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            po_no += item.get('PO_NO') + ',';
                        })
                        //if (po_no.length > 0)
                        //    po_no = po_no.substr(0, po_no.length - 1);


                        //console.log("wh_no=" + T1Query.getForm().findField('WH_NO').getValue()); // 560000
                        //console.log("wh_no=" + T1Query.getForm().findField('WH_NO').rawValue); // wh_no 560000 中央庫房衛材庫
                        //console.log("P1=" + T1Query.getForm().findField('P1').getValue()); // start_date P1=Sun Jun 18 2023 00:00:00 GMT+0800 (台北標準時間)
                        //console.log("P1=" + T1Query.getForm().findField('P1').rawValue); // 1120618
                        //console.log("P4=" + T1Query.getForm().findField('P4').getValue()); // end_date
                        //console.log("P4=" + T1Query.getForm().findField('P4').rawValue);
                        //console.log("P5=" + T1Query.getForm().findField('P5').getValue()); // mmcode [null|005ZYP04]
                        //console.log("P5=" + T1Query.getForm().findField('P5').rawValue); // mmcode [無|'津普速 口溶錠5毫克']
                        //console.log("P3=" + T1Query.getForm().findField('P3').getValue()); // agen_no [null|'001']
                        //console.log("P3=" + T1Query.getForm().findField('P3').rawValue); // agen_no [無|'112061900005']
                        //console.log("P2=" + T1Query.getForm().findField('P2').getValue()); // 訂單狀態 [無], PO_STATUS = T1Query.getForm().findField('P2').getValue(); // 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]
                        //console.log("P2=" + T1Query.getForm().findField('P2').rawValue); // 訂單狀態 ['全部']

                        var param = new Array();
                        param.push({ name: 'wh_no', value: T1Query.getForm().findField('WH_NO').getValue() }); // wh_no 庫房代碼
                        param.push({ name: 'start_date', value: T1Query.getForm().findField('P1').rawValue }); // start_date
                        param.push({ name: 'end_date', value: T1Query.getForm().findField('P4').rawValue }); // end_date
                        param.push({ name: 'po_no', value: po_no }); // 訂單編號
                        param.push({ name: 'mmcode', value: T1Query.getForm().findField('P5').getValue() }); // mmcode 院內碼 08021274
                        param.push({ name: 'agen_no', value: T1Query.getForm().findField('P3').getValue() }); // agen_no 廠商代碼
                        param.push({ name: 'po_status', value: T1Query.getForm().findField('P2').getValue() }); // 訂單狀態 [無], PO_STATUS = T1Query.getForm().findField('P2').getValue(); // 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]

                        PostForm('/api/BD0014/ExportDetailByZip', param);
                        msglabel('訊息區:匯出完成');
                    }
                }
            },
            {
                text: '設定傳真',
                id: 'btnFax',
                name: 'btnFax',
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

                        Ext.MessageBox.confirm('寄送MAIL', '要進行採購訂單設定傳真?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0014/SetFax',
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
            }
        ]
    });
    function sendEmail(po_nos) {
        Ext.Ajax.request({
            url: '/api/BD0014/SendEmail',
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

    function sendEmailAll() {
        Ext.Ajax.request({
            url: '/api/BD0014/SendMailAll',
            method: reqVal_p,
            params: {
                wh_no: T1Query.getForm().findField('WH_NO').getValue(), // wh_no
                start_date: T1Query.getForm().findField('P1').rawValue, // start_date
                end_date: T1Query.getForm().findField('P4').rawValue // end_date
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('一鍵寄送完成');
                    Ext.MessageBox.alert('一鍵寄送完成', '已處理' + data.msg + '筆訂單.');

                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
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
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        //selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox

        //    selectable: function (record) {
        //        return (record.get('PO_STATUS') != '開單' && record.get('PO_STATUS') != '已傳MAIL');
        //    },
        //},
        //),
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 140
            },
            {
                text: "物料類別",
                dataIndex: 'MAT_CLASS',
                width: 80
            },
            {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 80
            },
            {
                text: "EMAIL",
                dataIndex: 'EMAIL',
                width: 100
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 100
            },
            {
                text: "是否合約",
                dataIndex: 'M_CONTID',
                width: 80
            },
            {
                text: "採購筆數",
                dataIndex: 'CNT',
                width: 80,
                align: 'right',
                style: 'text-align:left',

            },
            {
                text: "訂單狀態",
                dataIndex: 'PO_STATUS',
                width: 80
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 350
            },
            {
                text: "超過十五萬元院內碼",
                dataIndex: 'MMCODE_OVER_150K',
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
                    T1Grid.down('#btnUpdate').setDisabled(true); // 修改
                    T1Grid.down('#btnSendMail').setDisabled(true); // 寄送MAIL
                    T1Grid.down('#btnReport').setDisabled(true); // 訂單列印
                    T1Grid.down('#btnReSendMail').setDisabled(true); // 補發mail
                    T1Grid.down('#btnObsolete').setDisabled(true); // 整份作廢
                    T1Grid.down('#btnExportDetailByExcel').setDisabled(true);  // ★匯出明細
                    T1Grid.down('#btnExportDetailByZip').setDisabled(true);  // ★匯出壓縮檔
                    T1Grid.down('#btnFax').setDisabled(true);

                    //if (T1Store.getCount() > 0) {
                    //    T1Grid.down('#btnReport').setDisabled(false);
                    //    T1Grid.down('#btnSendMail').setDisabled(false);
                    //}

                }
            }
        },
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {


                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T1LastRec = record;
                //console.log("record.data.PO_STATUS=" + record.data.PO_STATUS);
                var isTrue = true;
                if (record.data.PO_STATUS == '開單' || record.data.PO_STATUS == '已傳MAIL' || record.data.PO_STATUS == '待傳MAIL' || record.data.PO_STATUS == '已傳真') {
                    isTrue = false;
                }
                Ext.getCmp('btnUpdate').setDisabled(isTrue); // 修改
                Ext.getCmp('btnSendMail').setDisabled(isTrue); // 寄送MAIL
                Ext.getCmp('btnObsolete').setDisabled(isTrue); // 單筆作廢
                //Ext.getCmp('btnReSendMail').setDisabled(isTrue); // 補發mail
                T1Grid.down('#btnReport').setDisabled(isTrue); // 訂單列印
                T1Grid.down('#btnExportDetailByExcel').setDisabled(isTrue);  // ★匯出明細
                T1Grid.down('#btnExportDetailByZip').setDisabled(isTrue);  // ★匯出壓縮檔
                T1Grid.down('#btnFax').setDisabled(isTrue);

                if (record.data.PO_STATUS == '已傳MAIL') {
                    Ext.getCmp('btnSendMail').setDisabled(true); // 寄送MAIL
                    Ext.getCmp('btnReSendMail').setDisabled(false); // 補發mail
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
                    T1Set = '/api/BD0014/DetailUpdate';
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
                        let mmcode = ''; // 08021274
                        //selection.map(item => {
                        //    mmcode += item.get('MMCODE') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            mmcode += item.get('MMCODE') + ',';
                        })

                        Ext.MessageBox.confirm('單筆作廢', '要進行單筆作廢作業?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0014/DetailObsolete',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no,
                                        MMCODE: mmcode // 08021274
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料更新成功');

                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
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
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80,
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 100
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 100
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 60
            },
            {
                text: "包裝",
                dataIndex: 'M_PURUN',
                width: 60
            },
            {
                text: "數量",
                dataIndex: 'PO_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單價",
                dataIndex: 'PO_PRICE',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "小計",
                dataIndex: 'PO_AMT',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "病人姓名",
                dataIndex: 'CHINNAME',
                width: 100,
            },
            {
                text: "病歷號",
                dataIndex: 'CHARTNO',
                width: 100,
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 100,
            },
            {
                text: "買斷寄庫",
                dataIndex: 'E_SOURCECODE',
                width: 80,
            },
            {
                text: "合約案號",
                dataIndex: 'CASENO',
                width: 90,
            },
            {
                text: "合約到期日",
                dataIndex: 'E_CODATE',
                width: 90,
            },
            {
                text: "出貨單位",
                dataIndex: 'UNITRATE',
                width: 100,
                renderer: function (val, meta, record) {

                    return record.data.UNITRATE + ' ' + record.data.BASE_UNIT + '/' + record.data.M_PURUN;
                }
            },
            {
                text: "健保碼",
                dataIndex: 'M_NHIKEY',
                width: 100,
                renderer: function (val, meta, record) {

                    return record.data.M_NHIKEY;
                }
            },
            /*
            {
                text: "折讓比",
                dataIndex: 'M_DISCPERC',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            */
            {
                text: "成本價",
                dataIndex: 'DISC_CPRICE',
                width: 110,
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
                text: "分批交貨日期",
                dataIndex: 'PARTIALDL_DT',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                        //TATabs.setActiveTab('Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                viewport.down('#form').expand();
                T2Rec = records.length;
                T2LastRec = records[0];

                if (T2Grid.getSelection().length > 1) {
                    T2Grid.down('#btnUpdate2').setDisabled(true);
                }

                 setFormT2a();

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
        f.findField("MAT_CLASS").setValue(T1LastRec.data["MAT_CLASS"]);
        f.findField("M_CONTID").setValue(T1LastRec.data["M_CONTID"]);
        f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
        f.findField("AGEN_NAMEC").setValue(T1LastRec.data["AGEN_NAMEC"]);
        f.findField("PO_STATUS").setValue(T1LastRec.data["PO_STATUS"]);
        f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);

        f.findField('x').setValue(x);
        f.findField("MEMO_SELECT").setReadOnly(false);
        f.findField("MEMO").setReadOnly(false);

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
            f.findField("MAT_CLASS").setValue(T1LastRec.data["MAT_CLASS"]);
            f.findField("M_CONTID").setValue(T1LastRec.data["M_CONTID"]);
            f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
            f.findField("AGEN_NAMEC").setValue(T1LastRec.data["AGEN_NAMEC"]);
            f.findField("PO_STATUS").setValue(T1LastRec.data["PO_STATUS"]);
            f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);

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
                fieldLabel: '物料類別',
                name: 'MAT_CLASS',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '是否合約',
                name: 'M_CONTID'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '廠商代碼',
                name: 'AGEN_NO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '廠商名稱',
                name: 'AGEN_NAMEC',
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
                xtype: 'combo',
                fieldLabel: '備註快取',
                name: 'MEMO_SELECT',
                enforceMaxLength: true,
                labelWidth: 60,
                width: 281,
                padding: '0 4 0 4',
                store: memo_Store,
                displayField: 'VALUE',
                valueField: 'VALUE',
                queryMode: 'local',
                readOnly: true,
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div></tpl>',
                listeners: {
                    select: function (combo, records) {
                        var value = records.data.VALUE;
                        
                        T1Form.getForm().findField('MEMO').setValue(value);
                    }
                }
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'MEMO',
                enforceMaxLength: true,
                maxLength: 300,
                readOnly: true,
                width: "100%",
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
        T1Form.getForm().findField('MEMO_SELECT').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);
        f.findField('PARTIALDL_DT').setReadOnly(false);
        //若MI_MAST有修改UNITRATE則開放修改
        if (f.findField('UNITRATE').getValue() != f.findField('MAST_UNITRATE').getValue()) {
            f.findField('UNITRATE').setReadOnly(false);
        } else {
            f.findField('UNITRATE').setReadOnly(true);
        }
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

            //    f.findField("CONTRACNO").setValue(T1LastRec.data["CONTRACNO"]);
            //    f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
            f.findField("PO_NO").setValue(T1LastRec.data["PO_NO"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('M_PURUN').setValue(T2LastRec.data["M_PURUN"]);
            f.findField('PO_QTY').setValue(T2LastRec.data["PO_QTY"]);
            f.findField('PO_PRICE').setValue(T2LastRec.data["PO_PRICE"]);
            f.findField('PO_AMT').setValue(T2LastRec.data["PO_AMT"]);
            f.findField('MEMO').setValue(T2LastRec.data["MEMO"]);
            f.findField('E_SOURCECODE').setValue(T2LastRec.data["E_SOURCECODE"]);
            f.findField('CASENO').setValue(T2LastRec.data["CASENO"]);
            f.findField('E_CODATE').setValue(T2LastRec.data["E_CODATE"]);
            // f.findField('UNITRATE').setValue(T2LastRec.data["UNITRATE"] + ' ' + T2LastRec.data["BASE_UNIT"] + '/' + T2LastRec.data["M_PURUN"]);
            f.findField('UNITRATE').setValue(T2LastRec.data["UNITRATE"]);
            f.findField('M_DISCPERC').setValue(T2LastRec.data["M_DISCPERC"]);
            f.findField('DISC_CPRICE').setValue(T2LastRec.data["DISC_CPRICE"]);
            f.findField('MEMO').setValue(T2LastRec.data["MEMO"]);
            f.findField('STATUS').setValue(T2LastRec.data["STATUS"]);
            f.findField('MAST_UNITRATE').setValue(T2LastRec.data["MAST_UNITRATE"]);
            f.findField('CHARTNO').setValue(T2LastRec.data["CHARTNO"]);
            f.findField('CHINNAME').setValue(T2LastRec.data["CHINNAME"]);
            f.findField('PARTIALDL_DT').setValue(T2LastRec.data["PARTIALDL_DT"]);

        }
        var selections = T2Grid.getSelection();
        if (selections.length > 0) {
            var isTrue = true;
            if (T1LastRec.data.PO_STATUS == '開單' || T1LastRec.data.PO_STATUS == '已傳MAIL' || T1LastRec.data.PO_STATUS == '待傳MAIL') {
                isTrue = false;
            }
            T2Grid.down('#btnUpdate2').setDisabled(isTrue);
            T2Grid.down('#btnObsolete2').setDisabled(isTrue);
        } else {
            T2Grid.down('#btnUpdate2').setDisabled(true);
            T2Grid.down('#btnObsolete2').setDisabled(true);
            T2Form.getForm().reset();
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
        bodyPadding: '5 5 0',
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
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
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單位',
                        name: 'BASE_UNIT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '包裝',
                        name: 'M_PURUN',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
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
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單價',
                        name: 'PO_PRICE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '小計',
                        name: 'PO_AMT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '病人姓名',
                        name: 'CHINNAME',
                        enforceMaxLength: true,
                        maxLength: 2000,
                        readOnly: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '病歷號',
                        name: 'CHARTNO',
                        enforceMaxLength: true,
                        maxLength: 2000,
                        readOnly: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'textareafield',
                        fieldLabel: '備註',
                        name: 'MEMO',
                        enforceMaxLength: true,
                        maxLength: 2000,
                        readOnly: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '買斷寄庫',
                        name: 'E_SOURCECODE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '合約案號',
                        name: 'CASENO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '合約到期日',
                        name: 'E_CODATE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '出貨單位',
                        name: 'UNITRATE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        minValue: 1,
                        labelAlign: 'right'
                    }, {
                        xtype: 'label',
                        text: '數量必須為出貨單位的倍數',
                        style: 'color: #ff0000;'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '折讓比',
                        name: 'M_DISCPERC',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '優惠合約價',
                        name: 'DISC_CPRICE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '狀態',
                        name: 'STATUS',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'textareafield',
                        fieldLabel: '分批交貨日期',
                        name: 'PARTIALDL_DT',
                        enforceMaxLength: true,
                        maxLength: 50,
                        readOnly: true,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'hidden',
                        fieldLabel: 'UNITRATE',
                        name: 'MAST_UNITRATE',
                        readOnly: true
                    }
                ]
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var isSub = true;
                        var f = T2Form.getForm();
                        var unitrate = f.findField('UNITRATE').getValue();
                        var po_qty = f.findField('PO_QTY').getValue();
                        if (!(unitrate == null || unitrate == '' || unitrate == 0)) {
                            if (po_qty % unitrate == 0) {
                            }
                            else {
                                Ext.Msg.alert('提醒', '數量不能被出貨單位整除,請重新輸入');
                                isSub = false;
                            }
                        } else {
                            Ext.Msg.alert('提醒', '請確認出貨單位是否正確');
                            isSub = false;
                        }
                        if (isSub) {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T2Submit();

                                }
                            });
                        }
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
    function showReport(PO_NOs) {
        if (!win) {
            var PO_NOstr = '';
            $.each(PO_NOs, function (index, item) {
                PO_NOstr += item + ',';
            });
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                //html: '<iframe src="' + reportUrl + '?P0=' + T1LastRec.data.PO_NO + '" id="mainContent" name="mainContent" width="100% " height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                html: '<iframe src="' + reportUrl + '?P0=' + PO_NOstr + '" id="mainContent" name="mainContent" width="100% " height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        if (hospCode == '807') {
                            T1Load();
                            T2Store.removeAll();
                        }   
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
            { name: 'PO_AMT', type: 'string' },
            { name: 'SUM_PO_AMT', type: 'string' }
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
                text: "年度累積金額",
                dataIndex: 'SUM_PO_AMT',
                width: 100
            },
            {
                header: "",
                flex: 1
            }

        ],
    });
    var textPanel = Ext.create('Ext.panel.Panel', {
        html: '<p>確定並匯出:匯出超過十五萬元項目清單並繼續匯出作業或寄送信件<BR>取消:取消後續作業</p>',
        bodyPadding: 10,
        border: false
    });
    var over100KWindow = Ext.create('Ext.window.Window', {
        id: 'over100KWindow',
        renderTo: Ext.getBody(),
        items: [textPanel, T3Grid],
        modal: true,
        width: "800px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "超過十五萬元清單",
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
                    PostForm('../../../api/BD0014/Over150KExcel', p);

                    sendEmail(po_no)

                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();

                    over100KWindow.hide();
                }
            }
            , {
                text: '取消',
                handler: function () {
                    //console.log('取消');
                    over100KWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                over100KWindow.center();
                over100KWindow.setWidth(800);
            }
        }
    });
    over100KWindow.hide();
    //#endregion

    var sendAllChkWindow = Ext.create('Ext.window.Window', {
        id: 'sendAllChkWindow',
        renderTo: Ext.getBody(),
        items: [{
            xtype: 'panel',
            id: 'sendAllChk_PanelP1',
            border: false,
            layout: 'hbox',
            padding: '0 4 0 4',
            items: [
                {
                    xtype: 'textareafield',
                    fieldLabel: '檢查結果',
                    name: 'SENDALL_CHK_RESULT',
                    id: 'SENDALL_CHK_RESULT',
                    labelWidth: 80,
                    width: 600,
                    height: 300,
                    padding: '4 4 4 4'
                }
            ]
        }],
        modal: true,
        width: "630px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "一鍵寄送檢查結果",
        buttons: [
            {
                text: '確定並一鍵寄送',
                handler: function () {
                    Ext.MessageBox.confirm('一鍵寄送', '是否確定寄送包含年度金額達到定額的訂單?(廠商沒有EMAIL的訂單將不會進行寄送)', function (btn, text) {
                        if (btn === 'yes') {
                            sendEmailAll();

                            sendAllChkWindow.hide();
                        }
                    });
                }
            }, {
                text: '取消',
                handler: function () {
                    sendAllChkWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                sendAllChkWindow.center();
                sendAllChkWindow.setWidth(630);
            }
        }
    });
    sendAllChkWindow.hide();

    var editPOWindow = Ext.create('Ext.window.Window', {
        id: 'editPOWindow',
        renderTo: Ext.getBody(),
        items: [
            {
                xtype: 'panel',
                id: 'editPO_PanelP1',
                border: false,
                layout: 'hbox',
                padding: '0 4 0 4',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '物料類別',
                        name: 'MAT_CLASS',
                        id: 'MAT_CLASS',
                        labelWidth: 80,
                        width: 400,
                        padding: '4 4 4 4',
                        store: MatClassQueryStore,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        autoSelect: true,
                        anyMatch: true,
                        fieldCls: 'required',
                        allowBlank: false, // 欄位是否為必填
                        blankText: "請輸入物料類別",
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        listeners: {
                            afterrender: function (combo) {
                            },
                            select: function (combo, record, index) {
                                var selectedValue = record.get('VALUE');
                                var selectedText = record.get('TEXT');
                                //console.log('值：', selectedValue);
                                //console.log('內容：', selectedText);

                                var textareafield = Ext.getCmp('editPO_MEMO');
                                textareafield.setValue(""); // 當切換時，清空輸入內容

                                //取得【物料類別】的信件內容
                                Ext.Ajax.request({
                                    url: MatClassTextArea, // '../../../api/BD0014/GetMatClassTextArea';
                                    method: reqVal_p,
                                    params: {
                                        mat_class: selectedValue // MI_MAST.MAT_CLASS 物料分類代碼(01-藥品,02-衛材(含檢材),03-文具,04-清潔用品,05-表格,06-防護用具,07-被服,08-資訊耗材,09-氣體, 13-中藥)
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            var nos = data.etts;
                                            if (nos.length > 0) {
                                                var o = nos[0]; // BD0014M(DATA_NAME, DATA_VALUE, DATA_REMARK)
                                                var textareafield = Ext.getCmp('editPO_MEMO');
                                                textareafield.setValue(o.DATA_VALUE);
                                            }
                                        }
                                    },
                                    failure: function (response, options) {
                                        ;
                                    }
                                });
                            }
                        }
                    }
                ]
            }
            , {
                xtype: 'panel',
                id: 'editPO_PanelP2',
                border: false,
                layout: 'hbox',
                padding: '4 4 4 4',
                items: [
                    {
                        xtype: 'textareafield',
                        fieldLabel: '內容',
                        labelWidth: 80,
                        padding: '4 4 4 4',
                        id: 'editPO_MEMO',
                        name: 'editPO_MEMO',
                        enforceMaxLength: true,
                        maxLength: 300,
                        //readOnly: true,
                        width: "100%",
                        height: 200
                    }
                ]
            }
        ],
        modal: true,
        width: "600px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "信件內容維護",
        buttons: [
            {
                text: '儲存',
                handler: function () {
                    console.log('信件內容維護-儲存');
                    var mat_class = Ext.getCmp('MAT_CLASS').getValue(); // (MI_MAST.MAT_CLASS) 物料分類代碼(01-藥品,02-衛材(含檢材),03-文具,04-清潔用品,05-表格,06-防護用具,07-被服,08-資訊耗材,09-氣體, 13-中藥)"
                    var data_value = Ext.getCmp('editPO_MEMO').getValue();
                    Ext.Ajax.request({
                        url: '/api/BD0014/Set信件內容維護',
                        method: reqVal_p,
                        params: {
                            MAT_CLASS: '' + mat_class,
                            DATA_VALUE: data_value
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success == true) {
                                editPOWindow.hide();
                                msglabel('訂單訊息修改完成');
                            } else {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤-【訂單訊息】沒修改成功!!!');
                            }
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });


                }
            }
            , {
                text: '取消',
                handler: function () {
                    //console.log('信件內容維護-取消');
                    editPOWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                editPOWindow.center();
                editPOWindow.setWidth(600);
            }
        }
    });
    editPOWindow.hide();


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
    T1Grid.down('#btnUpdate').setDisabled(true);// 修改
    T1Grid.down('#btnSendMail').setDisabled(true); // 寄送MAIL
    T1Grid.down('#btnReport').setDisabled(true); // 訂單列印
    T1Grid.down('#btnObsolete').setDisabled(true); // 補發mail
    T1Grid.down('#btnReSendMail').setDisabled(true); // 整份作廢
    T1Grid.down('#btnExportDetailByExcel').setDisabled(true);  // ★匯出明細
    T1Grid.down('#btnExportDetailByZip').setDisabled(true);  // ★匯出壓縮檔
    T1Grid.down('#btnFax').setDisabled(true);

    T1Query.getForm().findField('P1').setValue(new Date());
    T1Query.getForm().findField('P4').setValue(new Date());



    function MenuLinkSet() {
        if (menuLink == "BD0024") {
            Ext.getCmp('btnUpdate').hide();
            Ext.getCmp('btnSendMail').hide();
            Ext.getCmp('btnReport').hide();
            Ext.getCmp('btnReSendMail').hide();
            Ext.getCmp('btnExportDetailByExcel').hide();
            Ext.getCmp('btnExportDetailByZip').hide();
            Ext.getCmp('btnUpdate2').hide();
            Ext.getCmp('btnObsolete2').hide();
            Ext.getCmp('btnMessageUpdate').hide();
            Ext.getCmp('btnSendMailAll').hide();
        }
        else if (menuLink == "BD0025") {
            Ext.getCmp('btnSendMail').hide();
            Ext.getCmp('btnReport').hide();
            Ext.getCmp('btnReSendMail').hide();
            Ext.getCmp('btnObsolete').hide();
            Ext.getCmp('btnExportDetailByExcel').hide();
            Ext.getCmp('btnExportDetailByZip').hide();
            Ext.getCmp('btnUpdate2').hide();
            Ext.getCmp('btnObsolete2').hide();
            Ext.getCmp('btnMessageUpdate').hide();
            Ext.getCmp('btnSendMailAll').hide();
        }
        else if (menuLink == "BD0026") {
            Ext.getCmp('btnUpdate').hide();
            Ext.getCmp('btnSendMail').hide();
            Ext.getCmp('btnReport').hide();
            Ext.getCmp('btnReSendMail').hide();
            Ext.getCmp('btnObsolete').hide();
            Ext.getCmp('btnExportDetailByExcel').hide();
            Ext.getCmp('btnExportDetailByZip').hide();
            Ext.getCmp('btnUpdate2').hide();
            Ext.getCmp('btnObsolete2').hide();
            Ext.getCmp('btnMessageUpdate').hide();
            Ext.getCmp('btnSendMailAll').hide();
            Ext.getCmp('btnFax').show();
        }
    }
    Ext.getCmp('btnFax').hide();
    MenuLinkSet();

    //松山沒有mail server, 會採用傳真, 若為松山則隱藏寄信相關按鈕
    function SetToolBtn() {
        Ext.Ajax.request({
            url: '/api/BD0014/ChkHospCode',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success == true) {
                    hospCode = data.msg;
                    if (hospCode == '807') {
                        Ext.getCmp('btnMessageUpdate').hide();
                        Ext.getCmp('btnSendMailAll').hide();
                        Ext.getCmp('btnSendMail').hide();
                        Ext.getCmp('btnReSendMail').hide();
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    SetToolBtn();
});