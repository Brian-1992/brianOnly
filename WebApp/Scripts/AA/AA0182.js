Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "批號效期維護";
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Set = '';
    var qtySet = '/api/AA0182/SetQty';
    var St_WhGet = '../../../api/AA0182/GetWhCombo';
    var St_MatclassGet = '../../../api/AA0182/GetMatclassCombo';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');

    // 庫房
    var st_wh = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 物料分類
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '院內碼',
        allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0182/GetMMCodeCombo',
        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        matchFieldWidth: false,
        listConfig: { width: 300 },
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T1Query.getForm();
            if (!f.findField("P1").readOnly) {
                var mat_class = f.findField("P1").getValue();
                return {
                    MAT_CLASS: mat_class
                };
            }
        },
        listeners: {
        }
    });

    function getMatClassDefaultValue(menuLink) {
        if (["AA0195", "AA0210", "AA0211", "AB0141"].includes(menuLink)) {
            return '01';
        } else if (menuLink === "AA0208") {
            return '02';
        } else {
            return null;
        }
    }

    function getMatClassDefaultDisabled(menuLink) {
        if (["AA0195", "AA0208", "AA0210", "AA0211"].includes(menuLink)) {
            return true;
        } else {
            return false;
        }
    }

    function getExpDateDefaultDisabled(menuLink) {
        if (["AA0210"].includes(menuLink)) {
            return true;
        } else {
            return false;
        }
    }

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
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
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '庫房',
                        id: 'P0',
                        name: 'P0',
                        store: st_wh,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        multiSelect: false,
                        width: 200,
                        matchFieldWidth: false,
                        listConfig: { width: 300 },
                        listeners: {
                            select: function (ele, newValue, oldValue) {
                                setComboMatClass();
                            }
                        }
                    },
                    {
                        xtype: 'tbspacer',
                        width: 20
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '物料分類',
                        id: 'P1',
                        name: 'P1',
                        store: st_matclass,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        multiSelect: false,
                        width: 200,
                        disabled: getMatClassDefaultDisabled(menuLink),
                    },
                    {
                        xtype: 'container',
                        layout: 'hbox',
                        items: [
                            mmCodeCombo,
                        ]
                    },
                    {
                        xtype: 'tbspacer',
                        width: 30
                    },
                    {
                        xtype: 'monthfield',
                        fieldLabel: '效期區間',
                        id: 'D0',
                        name: 'D0',
                        width: 160,
                        disabled: getExpDateDefaultDisabled(menuLink),
                    },
                    {
                        xtype: 'tbspacer',
                        width: 10
                    },
                    {
                        xtype: 'monthfield',
                        fieldLabel: '至',
                        id: 'D1',
                        name: 'D1',
                        labelWidth: 20,
                        width: 120,
                        disabled: getExpDateDefaultDisabled(menuLink),
                    }, {
                        xtype: 'checkbox',
                        boxLabel: '效期9991231',
                        id: 'P3',
                        name: 'P3',
                        labelWidth: 100,
                        width: 140,
                        margin: '0 0 0 10',
                        checked: true
                    },
                    {
                        xtype: 'tbspacer',
                        width: 10
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: T1Load,
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            T1Query.getForm().findField('P0').reset();
                            T1Query.getForm().findField('P1').reset();
                            T1Query.getForm().findField('P2').reset();
                            T1Query.getForm().findField('D0').reset();
                            T1Query.getForm().findField('D1').reset();

                            var f = this.up('form').getForm();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                            msglabel('訊息區:');
                        }
                    },
                    {
                        xtype: 'button',
                        text: '匯出',
                        name: 'excel',
                        id: 'excel',
                        handler: function () {
                            if (!T1Grid.getStore().getCount()) {
                                Ext.MessageBox.alert('訊息', '無資料');
                                return;
                            }

                            Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                                if (btn === 'yes') {
                                    var p = new Array();
                                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                                    p.push({ name: 'd0', value: T1Query.getForm().findField('D0').rawValue });
                                    p.push({ name: 'd1', value: T1Query.getForm().findField('D1').rawValue });
                                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });

                                    PostForm('/api/AA0182/GetExcel', p);
                                    msglabel('訊息區:匯出完成');
                                }
                            });


                        }
                    }
                ]
            }]
        }]
    });

    function setComboWhNo() {
        st_wh.removeAll();
        //庫房
        Ext.Ajax.request({
            url: St_WhGet,
            method: reqVal_p,
            params: {
                menuLink: menuLink
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_wh = data.etts;
                    if (tb_wh.length > 0) {
                        for (var i = 0; i < tb_wh.length; i++) {
                            st_wh.add({ VALUE: tb_wh[i].VALUE, COMBITEM: tb_wh[i].COMBITEM });
                        }
                    }
                    else {
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setComboMatClass() {
        st_matclass.removeAll();
        T1Query.getForm().findField('P1').setValue('');
        //物料分類
        Ext.Ajax.request({
            url: St_MatclassGet,
            method: reqVal_p,
            params: {
                p0: T1Query.getForm().findField('P0').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;
                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }
                        if (tb_matclass.length == 1) {
                            //1筆資料時將
                            T1Query.getForm().findField('P1').setValue(tb_matclass[0].VALUE);
                        }
                        else {
                        }
                    }
                    // 部分程式預設選項
                    T1Query.getForm().findField('P1').setValue(getMatClassDefaultValue(menuLink));
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setDefaultExpDate() {
        //效期區間
        Ext.Ajax.request({
            url: '/api/AA0182/GetExpDate',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_expDate = data.etts;
                    if (tb_expDate.length > 0) {
                        T1Query.getForm().findField('D0').setValue(tb_expDate[0].VALUE);
                        T1Query.getForm().findField('D1').setValue(tb_expDate[0].TEXT);
                    }
                    else {
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboWhNo();
    setComboMatClass();
    setDefaultExpDate();

    //T1Model        //定義有多少欄位參數
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' }, // 庫房代碼
            { name: 'WH_NAME', type: 'string' }, // 庫房名稱
            { name: 'MMCODE', type: 'string' },        // 院內碼
            { name: 'MMNAME_C', type: 'string' },        // 中文品名
            { name: 'MMNAME_E', type: 'string' },        // 英文品名
            { name: 'LOT_NO', type: 'string' },        // 批號
            { name: 'EXP_DATE', type: 'string' },        // 效期
            { name: 'INV_QTY', type: 'string' },        // 批號存量
            { name: 'WH_INV_QTY', type: 'string' },        // 庫房存量
            { name: 'EXP_CHK', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0182/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount == 0) {
                    msglabel('查無符合條件的資料!');
                }
            }
        }
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').rawValue);
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').rawValue);
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());

        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        displayMsg: "顯示{0} - {1}筆,共{2}筆       <span style=\'color:red\'>*效期在180天內以紅字顯示</span>",
        buttons: [
            {
                itemId: 'add', text: '新增', handler: function () {
                    T1Set = '/api/AA0182/Create';
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0182/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }, {
                itemId: 'update', text: '更新', handler: function () {
                    var store = T1Grid.getStore().data.items;

                    for (var i = 0; i < store.length; i++) {
                        // 批號存量是否有填寫
                        if (store[i].data.INV_QTY == '') {
                            Ext.Msg.alert('錯誤', '品項 ' + store[i].data.MMCODE + ' ' + store[i].data.LOT_NO + ' 批號存量未填寫完成，請重新確認');
                            return;
                        }
                    }

                    Ext.MessageBox.confirm('更新', '是否確定更新?', function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var list = [];
                            for (var i = 0; i < store.length; i++) {
                                if (store[i].dirty) {
                                    var item = {
                                        WH_NO: store[i].data.WH_NO,
                                        MMCODE: store[i].data.MMCODE,
                                        LOT_NO: store[i].data.LOT_NO,
                                        EXP_DATE: store[i].data.EXP_DATE,
                                        INV_QTY: store[i].data.INV_QTY
                                    }
                                    list.push(item);
                                }
                            }
                            if (list.length == 0) {
                                Ext.Msg.alert('訊息', '尚未填寫任何更新資料，請確認!');
                                myMask.hide();
                                return;
                            }
                            Ext.Ajax.request({
                                url: qtySet,
                                method: reqVal_p,
                                params: {
                                    list: Ext.util.JSON.encode(list)
                                },
                                success: function (response) {
                                    myMask.hide();
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel("更新完成");
                                        T1Load();
                                    } else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response, options) {
                                    myMask.hide();
                                }
                            });
                        }
                    }
                    );
                }
            }
        ]
    });

    function setFormT1(x, t) {

        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        var r = Ext.create('T1Model');
        T1Form.loadRecord(r);
        u = f.findField("MMCODE");
        u.setReadOnly(false);
        u.clearInvalid();
        u = f.findField("MMNAME_C");
        u.setValue("");
        u = f.findField("MMNAME_E");
        u.setValue("");
        u = f.findField("WH_NO");
        u.setReadOnly(false);
        u.clearInvalid();
        u = f.findField('INV_QTY');
        u.setValue(0);

        f.findField('EXP_DATE').setReadOnly(false);
        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE').clearInvalid();
        f.findField('LOT_NO').clearInvalid();
        f.findField('INV_QTY').setReadOnly(false);

        f.findField('x').setValue(x);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();

    }

    var clearFormForWhno = function () {
        var f = T1Form.getForm();
        var u = f.findField('MMCODE');
        u.setValue("");
        u.clearInvalid();
        u = f.findField('EXP_DATE');
        u.setValue("");
        u.clearInvalid();
        u = f.findField('LOT_NO');
        u.setValue("");
        u.clearInvalid();
        u = f.findField('INV_QTY');
        u.clearInvalid();
        u.setValue(0);
    }

    var T1AddMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0182/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        allowBlank: false, // 欄位為必填
        fieldCls: 'required',
        readOnly: true,
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                WH_NO: T1Form.getForm().findField('WH_NO').getValue(),  //p0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T1Form.getForm().findField('MMNAME_C').setValue(r.data.MMNAME_C);
                T1Form.getForm().findField('MMNAME_E').setValue(r.data.MMNAME_E);
            }
        },
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: 'right'
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            xtype: 'combo',
            store: st_wh,
            fieldLabel: '庫房代碼',
            name: 'WH_NO',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            queryMode: 'local',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required',
            multiSelect: false,
            listeners: {
                select: function (oldValue, newValue, eOpts) {

                    var wh_no = newValue.data.VALUE;
                    clearFormForWhno();
                }
            }
        },
            T1AddMMCode,
        {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        },
        {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        },
        {
            fieldLabel: '批號',
            name: 'LOT_NO',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'datefield',
            fieldLabel: '效期',
            name: 'EXP_DATE',
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '批號存量',
            name: 'INV_QTY',
            submitValue: true
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }

            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
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

                    var x = f.findField('x').getValue();
                    if (x === "I") {
                        var query = T1Query.getForm();
                        query.findField("P0").setValue(f.findField('WH_NO').getValue());
                        query.findField("P2").setValue(f.findField('MMCODE').getValue());
                        query.findField("D0").setValue(f.findField('EXP_DATE').getValue());
                        query.findField("D1").setValue(f.findField('EXP_DATE').getValue());
                        msglabel('訊息區:資料新增成功');
                    } else {
                        msglabel('訊息區:資料刪除成功');
                    }

                    T1Load();
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
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('');
        setFormT1a();

        //修改,刪除關閉
        T1Grid.down('#delete').setDisabled(true);

        viewport.down('#form').collapse();
    }

    function setFormT1a() {
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('WH_NO');
            u.clearInvalid();
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            u = f.findField('MMCODE');
            u.clearInvalid();
            u.setReadOnly(true);

            u = f.findField('LOT_NO');
            u.clearInvalid();
            u.setReadOnly(true);

            u = f.findField('EXP_DATE');
            u.clearInvalid();
            u.setReadOnly(true);

            u = f.findField('INV_QTY');
            u.clearInvalid();
            u.setReadOnly(true);
        }
        else {
            T1Form.getForm().reset();
        }
    }

    // 查詢結果資料列表
     //1130116 804要求若紅字則整行紅字
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                style: 'text-align:left',
                align: 'left',
                width: 110,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['WH_NO'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['WH_NO'];
                }
            },
            {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 150,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['WH_NAME'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['WH_NAME'];
                }
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['MMCODE'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['MMCODE'];
                }
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 350,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['MMNAME_C'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['MMNAME_C'];
                }
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                style: 'text-align:left',
                align: 'left',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['LOT_NO'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['LOT_NO'];
                }
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                style: 'text-align:left',
                align: 'left',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['EXP_DATE'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['EXP_DATE'];
                }
            },
            {
                text: "<b><font color=red>批號存量</font></b>",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                width: 100,
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^-?[0-9]+$/ // 用正規表示式限制可輸入內容
                }, align: 'right',
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['INV_QTY'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['INV_QTY'];
                }
            }, {
                text: "庫房存量",
                dataIndex: 'WH_INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['WH_INV_QTY'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['WH_INV_QTY'];
                }
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                    },
                    validateedit: function (editor, context, eOpts) {
                    }
                }
            })
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];

                if (T1LastRec) {
                    T1Form.loadRecord(T1LastRec);
                    T1Grid.down('#delete').setDisabled(false);
                }
                else {
                    var r = Ext.create('T1Model');
                    T1Form.loadRecord(r);
                    T1Grid.down('#delete').setDisabled(true);
                }
            }
        }
    });

    var viewport = Ext.create('Ext.Viewport', {
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
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            collapsed: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();

    

    
});