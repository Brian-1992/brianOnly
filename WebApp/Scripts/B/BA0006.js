Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var MATComboGet = '../../../api/BA0006/GetMATCombo';
    var MmcodeComboGet = '../../../api/BA0006/GetMmcodeCombo';
    var Wh_noComboGet = '../../../api/BA0006/GetWh_noCombo';

    var T1LastRec = null;

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var nValInput = '';
    var chg = 'Y';
    // 庫別清單
    var Wh_noQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 物料類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 院內碼類別清單
    var MmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var mats = data.etts;
                    if (mats.length > 0) {
                        for (var i = 0; i < mats.length; i++) {
                            MATQueryStore.add({ VALUE: mats[i].VALUE, TEXT: mats[i].TEXT });
                        }
                        T1Query.getForm().findField('P0').setValue(mats[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    function setComboData3() {
        Ext.Ajax.request({
            url: Wh_noComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_no = data.etts;
                    if (wh_no.length > 0) {
                        for (var i = 0; i < wh_no.length; i++) {
                            Wh_noQueryStore.add({ VALUE: wh_no[i].VALUE, TEXT: wh_no[i].TEXT });
                        }
                        t1Form.getForm().findField('WH_NO').setValue(wh_no[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData3();
    var T1FormMmcode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        queryUrl: MmcodeComboGet,
        limit: 500, //限制一次最多顯示10筆
        //extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                MAT_CLASS: T1Query.getForm().findField('P0').getValue()
            };
        },
        //storeAutoLoad: true,
        //insertEmptyRow: true,
        allowBlank: false,
        listeners: {
            change: function (combo, newValue, oldValue, eOpts) {
                if (chg == 'Y') {
                    Ext.Ajax.request({
                        url: '/api/BA0006/GetSelectMmcodeDetail',
                        method: reqVal_g,
                        params: {
                            MMCODE: newValue,
                            WH_NO: t1Form.getForm().findField('WH_NO').getValue()
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                var f = t1Form.getForm();
                                var tb_data = data.etts;
                                if (tb_data.length > 0) {
                                    f.findField('MMNAME_C').setValue(tb_data[0].MMNAME_C);
                                    f.findField('MMNAME_E').setValue(tb_data[0].MMNAME_E);
                                    if (tb_data[0].M_STOREID == '0') {  // 0:非庫備
                                        f.findField('M_STOREID0').setValue(true);
                                        f.findField('M_STOREID1').setValue(false);
                                    }
                                    else {
                                        f.findField('M_STOREID0').setValue(false);
                                        f.findField('M_STOREID1').setValue(true);
                                    }
                                    f.findField('ACC_QTY').setValue(0);
                                    f.findField('AGEN_NAMEC').setValue(tb_data[0].AGEN_NAMEC);
                                    f.findField('AGEN_NO').setValue(tb_data[0].AGEN_NO);
                                    if (tb_data[0].M_CONTID == '0') {  // 0:合約
                                        f.findField('M_CONTID0').setValue(true);
                                        f.findField('M_CONTID2').setValue(false);
                                    }
                                    else {
                                        f.findField('M_CONTID0').setValue(false);
                                        f.findField('M_CONTID2').setValue(true);
                                    }
                                    f.findField('M_CONTPRICE').setValue(tb_data[0].M_CONTPRICE);
                                    f.findField('APL_INQTY').setValue(tb_data[0].APL_INQTY);
                                    f.findField('UPRICE').setValue(tb_data[0].UPRICE);
                                    f.findField('M_PURUN').setValue(tb_data[0].M_PURUN);
                                    if (tb_data[0].UNIT_SWAP == null)
                                        Ext.Msg.alert('錯誤', '[轉換率]沒有值,請確認!');
                                    else
                                        f.findField('UNIT_SWAP').setValue(tb_data[0].UNIT_SWAP);
                                    f.findField('INV_QTY').setValue(tb_data[0].INV_QTY);
                                    f.findField('M_DISCPERC').setValue(tb_data[0].M_DISCPERC);
                                    f.findField('DISC_CPRICE').setValue(tb_data[0].DISC_CPRICE);
                                    f.findField('BASE_UNIT').setValue(tb_data[0].BASE_UNIT);
                                    f.findField('MAT_CLASS').setValue(tb_data[0].MAT_CLASS);
                                    f.findField('WEXP_ID').setValue(tb_data[0].WEXP_ID);
                                    f.findField('ACC_BASEUNIT').setValue(tb_data[0].BASE_UNIT);
                                    f.findField('ACC_PURUN').setValue(tb_data[0].M_PURUN);
                                }
                                f.findField("TX_QTY_T").setValue(0);
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }
        }
    });
    var T1Store = Ext.create('WEBAPP.store.BA0006', { // 定義於/Scripts/app/store/BA0006.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                            fieldLabel: '物料類別',
                            name: 'P0',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4',
                            store: MATQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '進貨日期',
                            name: 'P1',
                            labelWidth: mLabelWidth,
                            width: 150,
                            padding: '0 4 0 4',
                            fieldCls: 'required',
                            value: Ext.Date.add(new Date(), Ext.Date.DAY, -7)
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            name: 'P2',
                            labelWidth: 8,
                            width: 88,
                            padding: '0 4 0 4',
                            labelSeparator: '',
                            fieldCls: 'required',
                            value: new Date()

                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                var f = T1Query.getForm();
                                if (f.isValid()) {
                                    msglabel('訊息區:');
                                    T1Load();
                                    Ext.getCmp('btnDel').setDisabled(true);
                                }
                                else {
                                    Ext.MessageBox.alert('提示', '請輸入必填/必選資料後再進行查詢');
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
                        }]
                }

            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    T1Set = '/api/BA0006/CreateBC_CS_ACC_LOG';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                disabled: true,
                handler: function () {
                    T1Set = '/api/BA0006/updateBC_CS_ACC_LOG';
                    t1Form.getForm().findField("XACTION").setValue('U');
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    var po_no = '';
                    var seq = '';
                    if (selection.length) {
                        $.map(selection, function (item, key) {
                            po_no = item.get('PO_NO');
                            seq = item.get('SEQ');
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BA0006/DeleteBC_CS_ACC_LOG',
                                    params: {
                                        PO_NO: po_no,
                                        SEQ: seq
                                    },
                                    method: reqVal_p,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');
                                            t1Form.reset();
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
                        });
                    }
                }
            }
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
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
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "進貨日期",
                dataIndex: 'ACC_TIME',
                width: 80
            },
            {
                text: "入庫庫房",
                dataIndex: 'WH_NO',
                width: 70
            },
            {
                text: "進貨數量",
                dataIndex: 'ACC_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
             //{
            //    text: "包裝單位",
            //    dataIndex: 'ACC_PURUN',
            //    width: 80
            //},
            {
                text: "最小計量單位",
                dataIndex: 'BASE_UNIT',
                width: 90
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 150
            },
            {
                text: "廠商碼",
                dataIndex: 'AGEN_NO',
                width: 70,
            },
            {
                text: "狀態",
                dataIndex: 'STATUS',
                renderer: function (value, metaData, record, rowIndex) {
                    if (value == 'A' || value == 'C')
                        return '暫存';
                    else if (value == 'P')
                        return '已入帳';
                }
            },
            {
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                name: 'ACC_BASEUNIT',
                xtype: 'hidden'
            },
            {
                name: 'ACC_PURUN',
                xtype: 'hidden'
            },
            {
                name: 'PO_NO',
                xtype: 'hidden'
            },
            {
                name: 'INV_QTY',
                xtype: 'hidden'
            },
            {
                name: 'APL_INQTY',
                xtype: 'hidden'
            },
            {
                name: 'TX_QTY_T',
                xtype: 'hidden'
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec != null) {
                    setFormT1a();
                }
            }
        }
    });

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = t1Form.getForm();
        if (x == "I") {
            var r = Ext.create('WEBAPP.model.BA0006M'); // /Scripts/app/model/BA0006M.js
            t1Form.loadRecord(r); // 建立空白model,在新增時載入t1Form以清空欄位內容
            t1Form.getForm().findField("XACTION").setValue('I');
        }
        f.findField('x').setValue(x);
        t1Form.down('#cancel').setVisible(true);
        t1Form.down('#submit').setVisible(true);
        t1Form.down('#SubmitIn').setVisible(true);
        chg = 'Y';
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            chg = 'N';
            t1Form.loadRecord(T1LastRec);
            var f = t1Form.getForm();
            if (T1LastRec.data["M_STOREID"] == '0') {  // 0:非庫備
                f.findField('M_STOREID0').setValue(true);
                f.findField('M_STOREID1').setValue(false);
            }
            else {
                f.findField('M_STOREID0').setValue(false);
                f.findField('M_STOREID1').setValue(true);
            }
            if (T1LastRec.data["M_CONTID"] == '0') {  // 0:合約
                f.findField('M_CONTID0').setValue(true);
                f.findField('M_CONTID2').setValue(false);
            }
            else {
                f.findField('M_CONTID0').setValue(false);
                f.findField('M_CONTID2').setValue(true);
            }
            t1Form.down('#cancel').setVisible(false);
            t1Form.down('#submit').setVisible(false);
            t1Form.down('#SubmitIn').setVisible(false);
            Ext.getCmp('btnUpdate').setDisabled(false);
            if (T1LastRec.data["STATUS"] == 'A') {
                Ext.getCmp('btnUpdate').setDisabled(false);
                Ext.getCmp('btnDel').setDisabled(false);
            }
            else {
                Ext.getCmp('btnUpdate').setDisabled(true);
                Ext.getCmp('btnDel').setDisabled(true);
            }
            viewport.down('#form').expand();
        }
        else {
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('btnDel').setDisabled(true);
        }

    }

    // 顯示明細/新增/修改輸入欄
    var t1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0 0',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
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
                        xtype: 'combobox',
                        name: 'WH_NO',
                        fieldLabel: '入庫庫房',
                        store: Wh_noQueryStore,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        editable: false, typeAhead: true, forceSelection: true,
                        matchFieldWidth: true,
                        submitValue: true,
                        allowBlank: false,
                        width: 270
                    },
                    T1FormMmcode,
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        readOnly: true,
                        submitValue: true,
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商碼',
                        name: 'AGEN_NO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商中文名稱',
                        name: 'AGEN_NAMEC',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '庫存量',
                        name: 'INV_QTY',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'radiogroup',
                        fieldLabel: '是否合約',
                        name: 'M_CONTID',
                        enforceMaxLength: true,
                        readOnly: true,
                        items: [
                            { boxLabel: '是', name: 'M_CONTID', id: 'M_CONTID0', readOnly: true, inputValue: '0', width: 70, checked: true },
                            { boxLabel: '否', name: 'M_CONTID', id: 'M_CONTID2', readOnly: true, inputValue: '2' }
                        ]
                    },
                    {
                        xtype: 'radiogroup',
                        fieldLabel: '是否庫備',
                        name: 'M_STOREID',
                        enforceMaxLength: true,
                        readOnly: true,
                        items: [
                            { boxLabel: '庫備', name: 'M_STOREID', id: 'M_STOREID1', readOnly: true, inputValue: '1', width: 70, checked: true },
                            { boxLabel: '非庫備', name: 'M_STOREID', id: 'M_STOREID0', readOnly: true, inputValue: '0' }
                        ]
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最小計量單位',
                        name: 'BASE_UNIT',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '入帳數量',
                        name: 'TX_QTY_T',
                        //minValue: 1,
                        fieldCls: 'required',
                        allowBlank: false,
                        listeners: {
                            change: function (_this, newvalue, oldvalue) {
                                t1Form.getForm().findField('ACC_QTY').setValue(newvalue * t1Form.getForm().findField('UNIT_SWAP').getValue());
                            }
                        },
                        validator: function (value) {
                            if (value == 0) {
                                return '[入帳數量]不可輸入"0" !';
                            }
                            return true;
                        }
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '包裝單位',
                        name: 'M_PURUN',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '轉換率',
                        name: 'UNIT_SWAP',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '進貨數量',
                        name: 'ACC_QTY',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最小單價',
                        name: 'UPRICE',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '合約價',
                        name: 'M_CONTPRICE',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '折讓',
                        name: 'M_DISCPERC',
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '本月進貨',
                        name: 'APL_INQTY',
                        readOnly: true
                    },
                    {
                        xtype: 'textarea',
                        fieldLabel: '備註',
                        name: 'MEMO',
                        submitValue: true,
                        maxLength: 300
                    },
                    {
                        xtype: 'hidden',
                        fieldLabel: 'MAT_CLASS',
                        name: 'MAT_CLASS'
                    },
                    {
                        xtype: 'hidden',
                        fieldLabel: 'DISC_CPRICE',
                        name: 'DISC_CPRICE'
                    },
                    {
                        xtype: 'hidden',
                        fieldLabel: 'WEXP_ID',
                        name: 'WEXP_ID'
                    },
                    {
                        xtype: 'hidden',
                        name: 'SEQ'
                    },
                    {
                        xtype: 'hidden',
                        name: 'ACC_BASEUNIT'
                    },
                    {
                        xtype: 'hidden',
                        name: 'ACC_PURUN'
                    },
                    {
                        xtype: 'hidden',
                        name: 'STATUS'
                    },
                    {
                        xtype: 'hidden',
                        name: 'PO_NO'
                    },
                    {
                        xtype: 'hidden',
                        name: 'XACTION'
                    },
                    {
                        xtype: 'hidden',
                        name: 'INID'
                    },
                    {
                        xtype: 'hidden',
                        name: 'DISC_UPRICE'
                    }
                ]
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '暫存',
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        var f = t1Form.getForm();
                        var unit_swap = f.findField('UNIT_SWAP').getValue();
                        var acc_qty = f.findField('ACC_QTY').getValue();
                        if (!(unit_swap == null || unit_swap == '' || unit_swap == 0)) {
                            if (acc_qty % unit_swap == 0)
                                T1Submit();
                            else {
                                Ext.Msg.alert('提醒', '數量不能被轉換率整除,請重新輸入');
                            }
                        } else {
                            Ext.Msg.alert('提醒', '請確認轉換率是否正確');
                        }
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'SubmitIn', text: '入庫',
                handler: function () {
                    t1Form.down('#SubmitIn').setVisible(false);
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        var f = t1Form.getForm();
                        var unit_swap = f.findField('UNIT_SWAP').getValue();
                        var acc_qty = f.findField('ACC_QTY').getValue();
                        if (!(unit_swap == null || unit_swap == '' || unit_swap == 0)) {
                            if (acc_qty % unit_swap == 0) {
                                if (acc_qty > 0)
                                    T1SubmitIn();
                                else {
                                    var msg = '';
                                    if (acc_qty < 0)
                                        msg = '[入帳數量]為負值將減少庫存量 !';
                                    else
                                        msg = '將執行入庫增加庫存量 !';
                                    Ext.MessageBox.confirm('提醒', msg, function (btn, text) {
                                        if (btn === 'yes') {
                                            T1SubmitIn();
                                        }
                                    });
                                }
                            }
                            else {
                                Ext.Msg.alert('提醒', '數量不能被轉換率整除,請重新輸入');
                            }
                        } else {
                            Ext.Msg.alert('提醒', '請確認轉換率是否正確');
                        }
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        t1Form.down('#SubmitIn').setVisible(true);
                    }
                }
            },
            {
                itemId: 'cancel', text: '離開', hidden: true, handler: T1Cleanup
            }
        ]
    });

    function T1Submit() {
        var f = t1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                params: {
                },
                success: function (form, action) {
                    var f2 = t1Form.getForm();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel("暫存成功");
                            break;
                        case "U":
                            msglabel("資料更新完成");
                            break;
                    }
                    T1Cleanup();
                    T1Load();
                    myMask.hide();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', '資料輸入錯誤');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', '網頁連線失敗');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
        chg = 'Y';
    }
    function T1SubmitIn() {
        var f = t1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: '/api/BA0006/InWHNO',
                success: function (form, action) {
                    myMask.hide();
                    var data = Ext.decode(action.response.responseText);
                    if (data.msg == '入庫除帳..完成') {
                        Ext.Msg.alert('訊息', '入庫除帳..完成');
                        msglabel('訊息區:入庫除帳完成');
                        var f2 = t1Form.getForm();
                        switch (f2.findField("x").getValue()) {
                            case "I":
                                setFormT1('I', '新增');
                                break;
                            case "U":
                                T1Cleanup();
                                break;
                        }
                        t1Form.down('#submit').setVisible(false);
                        t1Form.down('#SubmitIn').setVisible(false);
                    }
                    else
                        Ext.Msg.alert('失敗', '入庫除帳..失敗' + data.msg);
                    T1Load();
                    myMask.hide();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', '資料輸入錯誤');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', '網頁連線失敗');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', '入庫除帳..失敗' + action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = t1Form.getForm();
        f.reset();
        t1Form.down('#cancel').hide();
        t1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        T1Grid.getSelectionModel().deselectAll();
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
                        height: '100%',
                        split: true,
                        items: [T1Grid]
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
                items: [t1Form]
            }
        ]
    });

});